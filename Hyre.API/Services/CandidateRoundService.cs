using Azure.Core;
using Hyre.API.Data;
using Hyre.API.Dtos.Scheduling;
using Hyre.API.Interfaces;
using Hyre.API.Interfaces.Candidates;
using Hyre.API.Interfaces.Scheduling;
using Hyre.API.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Hyre.API.Services
{
    public class CandidateRoundService : ICandidateRoundService
    {
        private readonly ICandidateRoundRepository _roundRepo;
        private readonly ApplicationDbContext _context;
        private readonly ICandidateInterviewService _interviewService;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ICandidateService _candidateService;
        private readonly IJobService _jobService;

        public CandidateRoundService(ICandidateRoundRepository roundRepo, ApplicationDbContext context, ICandidateInterviewService interviewService, UserManager<ApplicationUser> userManager, ICandidateService candidateService, IJobService jobService)
        {
            _roundRepo = roundRepo;
            _context = context;
            _interviewService = interviewService;
            _userManager = userManager;
            _candidateService = candidateService;
            _jobService = jobService;
        }

        public async Task<List<CandidateRoundDto>> GetCandidateRoundsAsync(int candidateId, int jobId)
        {
            var candidateExists = await _candidateService.CandidateExistsAsync(candidateId);
            if (!candidateExists)
                throw new Exception($"Candidate with ID {candidateId} not found");

            var job = await _jobService.GetJobByIdAsync(jobId);
            if (job == null)
                throw new Exception($"Job with ID {jobId} not found");

            var rounds = await _roundRepo.GetByCandidateAndJobAsync(candidateId, jobId);
            return rounds.Select(r => new CandidateRoundDto(
                r.CandidateRoundID,
                r.SequenceNo,
                r.RoundName,
                r.RoundType,
                r.IsPanelRound,
                r.PanelMembers?.Select(pm => pm.InterviewerID).ToList() ?? new List<string>(),
                r.InterviewMode,
                r.ScheduledDate,
                r.StartTime,
                r.DurationMinutes ?? 0,
                r.Status,
                null
            )).ToList();
        }

        public async Task<UpsertRoundResponseDto> UpsertCandidateRoundsAsync(CandidateRoundsUpdateDto dto, string recruiterId)
        {
            if (dto == null) throw new ArgumentNullException(nameof(dto));

            var candidateExists = await _candidateService.CandidateExistsAsync(dto.CandidateId);
            if (!candidateExists)
                throw new Exception($"Candidate with ID {dto.CandidateId} not found");

            var job = await _jobService.GetJobByIdAsync(dto.JobId);
            if (job == null)
                throw new Exception($"Job with ID {dto.JobId} not found");

            using var tx = await _context.Database.BeginTransactionAsync();
            try
            {

                var tempIdMap = new Dictionary<string, int>();

                var existingRounds = await _context.CandidateInterviewRounds.Include(r => r.PanelMembers)
                    .Where(r => r.CandidateID == dto.CandidateId && r.JobID == dto.JobId).ToListAsync();

                var existingIds = existingRounds.Select(r => r.CandidateRoundID).ToHashSet();
                var incomingIds = dto.Rounds.Where(r => r.CandidateRoundId.HasValue).Select(r => r.CandidateRoundId!.Value).ToHashSet();

                // 1) Delete first 
                var idsToDelete = existingIds.Except(incomingIds).ToList();
                if (idsToDelete.Any())
                {
                    // delete panel members
                    var membersToDelete = await _context.CandidatePanelMembers.Where(pm => idsToDelete.Contains(pm.CandidateRoundID)).ToListAsync();
                    if (membersToDelete.Any()) _context.CandidatePanelMembers.RemoveRange(membersToDelete);

                    var roundsToDelete = existingRounds.Where(r => idsToDelete.Contains(r.CandidateRoundID)).ToList();
                    if (roundsToDelete.Any()) _context.CandidateInterviewRounds.RemoveRange(roundsToDelete);

                    await _context.SaveChangesAsync();
                }

                existingRounds = await _context.CandidateInterviewRounds.Include(r => r.PanelMembers)
                    .Where(r => r.CandidateID == dto.CandidateId && r.JobID == dto.JobId).ToListAsync();
                var existingById = existingRounds.ToDictionary(r => r.CandidateRoundID);

                // 2) Update existing ones
                var roundsToUpdate = dto.Rounds.Where(r => r.CandidateRoundId.HasValue).ToList();
                foreach (var inc in roundsToUpdate)
                {
                    var id = inc.CandidateRoundId!.Value;
                    if (!existingById.ContainsKey(id)) throw new InvalidOperationException($"Round id {id} not found.");

                    var dbRound = existingById[id];
                    dbRound.SequenceNo = inc.SequenceNo;
                    dbRound.RoundName = inc.RoundName;
                    dbRound.RoundType = inc.RoundType;
                    dbRound.IsPanelRound = inc.IsPanelRound;
                    dbRound.InterviewMode = inc.InterviewMode;
                    dbRound.ScheduledDate = inc.ScheduledDate;
                    dbRound.StartTime = inc.StartTime;
                    dbRound.DurationMinutes = inc.DurationMinutes;
                    dbRound.Status = inc.Status ?? dbRound.Status;
                    dbRound.UpdatedAt = DateTime.UtcNow;
                    dbRound.RecruiterID = recruiterId;

                    /**/
                    // Handle meeting link based on interview mode
                    if (!string.IsNullOrEmpty(inc.InterviewMode) &&
                        inc.InterviewMode.Equals("Online", StringComparison.OrdinalIgnoreCase))
                    {
                        // Generate meeting link if it's an online interview and doesn't have one
                        if (string.IsNullOrEmpty(dbRound.MeetingLink))
                        {
                            dbRound.MeetingLink = GenerateJitsiLink(dto.JobId, dto.CandidateId, dbRound.CandidateRoundID);
                        }
                    }
                    else
                    {
                        dbRound.MeetingLink = null;
                    }
                    /**/

                    // Update panel members
                    if (dbRound.IsPanelRound)
                    {
                        var incomingMemberIds = (inc.InterviewerIds ?? new List<string>()).Distinct().ToList();
                        var existingMemberIds = dbRound.PanelMembers?.Select(pm => pm.InterviewerID).ToList() ?? new List<string>();

                        var remove = dbRound.PanelMembers.Where(pm => !incomingMemberIds.Contains(pm.InterviewerID)).ToList();
                        if (remove.Any()) _context.CandidatePanelMembers.RemoveRange(remove);

                        var add = incomingMemberIds.Except(existingMemberIds).Select(idm => new CandidatePanelMember { CandidateRoundID = dbRound.CandidateRoundID, InterviewerID = idm }).ToList();
                        if (add.Any()) await _context.CandidatePanelMembers.AddRangeAsync(add);
                    }
                    else
                    {
                        dbRound.InterviewerID = inc.InterviewerIds?.FirstOrDefault();
                        if (dbRound.PanelMembers != null && dbRound.PanelMembers.Any())
                            _context.CandidatePanelMembers.RemoveRange(dbRound.PanelMembers);
                    }
                }

                await _context.SaveChangesAsync();

                // 3) Insert new rounds -> call scheduler for each new incoming round
                var newIncoming = dto.Rounds.Where(r => !r.CandidateRoundId.HasValue).ToList();
                var createdEntities = new List<CandidateInterviewRound>();

                foreach (var inc in newIncoming)
                {
                    var roundCreate = new RoundCreateDto(inc.SequenceNo, inc.RoundName, inc.IsPanelRound, inc.InterviewerIds ?? new List<string>(), inc.InterviewMode, inc.ScheduledDate, inc.StartTime, inc.DurationMinutes, inc.RoundType);
                    var created = await _interviewService.ScheduleSingleRoundAsync(roundCreate, dto.CandidateId, dto.JobId, recruiterId, saveChanges: false);
                    createdEntities.Add(created);

                   /* // MAP CLIENT TEMP ID → REAL DB ID
                    if (!string.IsNullOrEmpty(inc.ClientTempId))
                        tempIdMap[inc.ClientTempId] = created.CandidateRoundID;*/
                }

                await _context.SaveChangesAsync();

                for (int i = 0; i < newIncoming.Count; i++)
                {
                    var inc = newIncoming[i];
                    var created = createdEntities[i];

                    if (!string.IsNullOrEmpty(inc.ClientTempId))
                        tempIdMap[inc.ClientTempId] = created.CandidateRoundID;

                    if (string.IsNullOrEmpty(created.MeetingLink) &&
                        !string.IsNullOrEmpty(created.InterviewMode) &&
                        created.InterviewMode.Equals("Online", StringComparison.OrdinalIgnoreCase))
                    {
                        created.MeetingLink = GenerateJitsiLink(dto.JobId, dto.CandidateId, created.CandidateRoundID);
                    }
                }

                var finalRounds = await _context.CandidateInterviewRounds.Include(r => r.PanelMembers)
                    .Where(r => r.CandidateID == dto.CandidateId && r.JobID == dto.JobId)
                    .OrderBy(r => r.SequenceNo).ToListAsync();

                for (int i = 0; i < finalRounds.Count; i++)
                {
                    var expected = i + 1;
                    if (finalRounds[i].SequenceNo != expected)
                    {
                        finalRounds[i].SequenceNo = expected;
                        finalRounds[i].UpdatedAt = DateTime.UtcNow;
                    }
                }

                await _context.SaveChangesAsync();
                await tx.CommitAsync();

                var resultRounds = finalRounds.Select(r =>
                {
                    var panelIds = r.PanelMembers?
                        .Select(pm => pm.InterviewerID)
                        .ToList() ?? new List<string>();

                    panelIds.Add(r.InterviewerID);

                    return new CandidateRoundDto(
                        r.CandidateRoundID,
                        r.SequenceNo,
                        r.RoundName,
                        r.RoundType,
                        r.IsPanelRound,
                        panelIds,
                        r.InterviewMode,
                        r.ScheduledDate,
                        r.StartTime,
                        r.DurationMinutes ?? 0,
                        r.Status,
                        null
                    );
                }).ToList();


                return new UpsertRoundResponseDto(resultRounds, tempIdMap);
            }
            catch
            {
                await tx.RollbackAsync();
                throw;
            }
        }

        private string GenerateJitsiLink(int jobId, int candidateId, int roundId)
        {
            var token = Guid.NewGuid().ToString("n").Substring(0, 8);
            return $"https://meet.jit.si/hyre-job{jobId}-cand{candidateId}-r{roundId}-{token}";
        }

        public async Task<List<JobScheduleStateDto>> GetJobsWithSchedulingStateAsync()
        {
            var jobs = await _context.Jobs
                .Where(j => j.Status == "Open")
                .ToListAsync();

            var pendingProfilesCounts = await _context.CandidateInterviewRounds
                .Where(r => r.Job.Status == "Open" && 
                    (
                        r.Status == "Pending" ||

                        (r.Status != "Completed" && r.Status != "Cancelled" &&
                         (r.ScheduledDate == null || r.StartTime == null ||
                          (!r.IsPanelRound && r.InterviewerID == null) ||
                          (r.IsPanelRound && !r.PanelMembers.Any()))
                        )
                    )
                )
                .GroupBy(r => r.JobID)
                .Select(g => new { JobID = g.Key, Count = g.Select(r => r.CandidateID).Distinct().Count() })
                .ToDictionaryAsync(x => x.JobID, x => x.Count);

            return jobs.Select(job => new JobScheduleStateDto(
                job.JobID,
                job.Title,
                job.Description ?? string.Empty,
                job.CompanyName,
                job.Location ?? string.Empty,
                job.JobType,
                job.WorkplaceType,
                job.Status,
                job.MinExperience,
                job.MaxExperience,
                job.CreatedAt,
                pendingProfilesCounts.GetValueOrDefault(job.JobID, 0)
            )).ToList();
        }
    }
}
