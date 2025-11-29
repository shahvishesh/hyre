using Hyre.API.Data;
using Hyre.API.Dtos.Scheduling;
using Hyre.API.Interfaces.Scheduling;
using Hyre.API.Models;

namespace Hyre.API.Services
{
    public class CandidateInterviewService : ICandidateInterviewService
    {
        private readonly ICandidateInterviewRepository _repo;
        //private readonly IInterviewScheduleRepository _availabilityRepo;
        private readonly TimeSpan BreakGap = TimeSpan.FromMinutes(30);
        private const int MaxInterviewsPerDay = 3;
        private readonly ApplicationDbContext _context;

        public CandidateInterviewService(
            ICandidateInterviewRepository repo,
            IInterviewScheduleRepository availabilityRepo,
            ApplicationDbContext context)
        {
            _repo = repo;
            //_availabilityRepo = availabilityRepo;
            _context = context;
        }

        public async Task<List<ScheduleResultDto>> ScheduleRoundsAsync(CreateCandidateInterviewDto dto, string recruiterId)
        {
            var results = new List<ScheduleResultDto>();


            var datesToCheck = dto.TechnicalRounds.Select(r => r.ScheduledDate?.Date)
                .Concat(dto.HrRound != null ? new DateTime?[] { dto.HrRound.ScheduledDate?.Date } : Array.Empty<DateTime?>())
                .Where(d => d.HasValue)
                .Select(d => d!.Value.Date)
                .Distinct();

            foreach (var date in datesToCheck)
            {
                var candidateCount = await _repo.CountCandidateInterviewsOnDateAsync(dto.CandidateId, date);
                if (candidateCount >= MaxInterviewsPerDay)
                    throw new InvalidOperationException($"Candidate already has {MaxInterviewsPerDay} interviews on {date:d}");
            }

            foreach (var round in dto.TechnicalRounds.OrderBy(r => r.SequenceNo))
            {
                if (!round.ScheduledDate.HasValue || !round.StartTime.HasValue)
                    throw new InvalidOperationException("Round must have ScheduledDate and StartTime.");

                var scheduledStart = round.ScheduledDate.Value.Date + round.StartTime.Value;
                var scheduledEnd = scheduledStart.AddMinutes(round.DurationMinutes);

                ValidateDateRules(scheduledStart);

                if (!await _repo.IsCandidateAvailableAsync(dto.CandidateId, scheduledStart, scheduledEnd))
                    throw new InvalidOperationException($"Candidate not available for round '{round.RoundName}' at {scheduledStart}.");

                var candidateCount = await _repo.CountCandidateInterviewsOnDateAsync(dto.CandidateId, scheduledStart.Date);
                if (candidateCount >= MaxInterviewsPerDay)
                    throw new InvalidOperationException($"Candidate already has {MaxInterviewsPerDay} interviews on {scheduledStart.Date:d}");

                if (round.IsPanelRound)
                {
                    foreach (var interviewerId in round.InterviewerIds.Distinct())
                    {
                        var count = await _repo.CountInterviewerInterviewsOnDateAsync(interviewerId, scheduledStart.Date);
                        if (count >= MaxInterviewsPerDay)
                            throw new InvalidOperationException($"Interviewer {interviewerId} already has {MaxInterviewsPerDay} interviews on {scheduledStart.Date:d}");

                        var startWithBeforeBuffer = scheduledStart - BreakGap;
                        var endWithAfterBuffer = scheduledEnd + BreakGap;

                        if (!await _repo.IsInterviewerAvailableAsync(interviewerId, startWithBeforeBuffer, endWithAfterBuffer))
                            throw new InvalidOperationException($"Interviewer {interviewerId} is not available for panel round '{round.RoundName}' at {scheduledStart}.");
                    }
                }
                else
                {
                    var interviewerId = round.InterviewerIds?.FirstOrDefault()
                        ?? throw new InvalidOperationException("Single interviewer round must have one interviewer.");

                    var count = await _repo.CountInterviewerInterviewsOnDateAsync(interviewerId, scheduledStart.Date);
                    if (count >= MaxInterviewsPerDay)
                        throw new InvalidOperationException($"Interviewer {interviewerId} already has {MaxInterviewsPerDay} interviews on {scheduledStart.Date:d}");

                    var startWithBeforeBuffer = scheduledStart - BreakGap;
                    var endWithAfterBuffer = scheduledEnd + BreakGap;

                    if (!await _repo.IsInterviewerAvailableAsync(interviewerId, startWithBeforeBuffer, endWithAfterBuffer))
                        throw new InvalidOperationException($"Interviewer {interviewerId} is not available for round '{round.RoundName}' at {scheduledStart}.");
                }

                var roundEntity = new CandidateInterviewRound
                {
                    CandidateID = dto.CandidateId,
                    JobID = dto.JobId,
                    SequenceNo = round.SequenceNo,
                    RoundName = round.RoundName,
                    RoundType = "Technical",
                    IsPanelRound = round.IsPanelRound,
                    RecruiterID = recruiterId,
                    ScheduledDate = round.ScheduledDate.Value.Date,
                    StartTime = round.StartTime.Value,
                    DurationMinutes = round.DurationMinutes,
                    InterviewMode = round.InterviewMode,
                    Status = "Scheduled",
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                await _repo.AddCandidateInterviewRoundAsync(roundEntity);
                await _repo.SaveChangesAsync(); // save to get ID for panel members

                if (round.IsPanelRound)
                {
                    var members = round.InterviewerIds.Distinct()
                        .Select(id => new CandidatePanelMember
                        {
                            CandidateRoundID = roundEntity.CandidateRoundID,
                            InterviewerID = id
                        }).ToList();

                    await _repo.AddCandidatePanelMembersAsync(members);
                }
                else
                {
                    roundEntity.InterviewerID = round.InterviewerIds.First();
                    // update
                    await _repo.SaveChangesAsync();
                }

                // Generate meeting link
                var meetingLink = GenerateJitsiLink(dto.JobId, dto.CandidateId, roundEntity.CandidateRoundID);
                roundEntity.MeetingLink = meetingLink;
                await _repo.SaveChangesAsync();

                results.Add(new ScheduleResultDto(
                    roundEntity.CandidateRoundID,
                    dto.CandidateId,
                    scheduledStart.ToUniversalTime(),
                    scheduledEnd.ToUniversalTime(),
                    meetingLink
                ));
            }

            // HR Round 
            if (dto.HrRound != null)
            {
                var hr = dto.HrRound;
                if (!hr.ScheduledDate.HasValue || !hr.StartTime.HasValue)
                    throw new InvalidOperationException("HR round must have date and start time.");

                var hrStart = hr.ScheduledDate.Value.Date + hr.StartTime.Value;
                var hrEnd = hrStart.AddMinutes(hr.DurationMinutes);

                ValidateDateRules(hrStart);

                if (!await _repo.IsCandidateAvailableAsync(dto.CandidateId, hrStart, hrEnd))
                    throw new InvalidOperationException($"Candidate not available for HR round at {hrStart}.");

                var hrInterviewer = hr.InterviewerIds?.FirstOrDefault()
                    ?? throw new InvalidOperationException("HR round must have an HR interviewer.");

                var count = await _repo.CountInterviewerInterviewsOnDateAsync(hrInterviewer, hrStart.Date);
                if (count >= MaxInterviewsPerDay)
                    throw new InvalidOperationException($"HR Interviewer already has {MaxInterviewsPerDay} interviews on {hrStart.Date:d}");

                var startWithBeforeBuffer = hrStart - BreakGap;
                var endWithAfterBuffer = hrEnd + BreakGap;

                if (!await _repo.IsInterviewerAvailableAsync(hrInterviewer, startWithBeforeBuffer, endWithAfterBuffer))
                    throw new InvalidOperationException($"HR interviewer {hrInterviewer} is not available at {hrStart}.");

                var hrEntity = new CandidateInterviewRound
                {
                    CandidateID = dto.CandidateId,
                    JobID = dto.JobId,
                    SequenceNo = hr.SequenceNo,
                    RoundName = hr.RoundName,
                    RoundType = "HR",
                    IsPanelRound = false,
                    RecruiterID = recruiterId,
                    InterviewerID = hrInterviewer,
                    ScheduledDate = hr.ScheduledDate.Value.Date,
                    StartTime = hr.StartTime.Value,
                    DurationMinutes = hr.DurationMinutes,
                    InterviewMode = hr.InterviewMode,
                    Status = "Scheduled",
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                await _repo.AddCandidateInterviewRoundAsync(hrEntity);
                await _repo.SaveChangesAsync();

                var meetingLink = GenerateJitsiLink(dto.JobId, dto.CandidateId, hrEntity.CandidateRoundID);
                hrEntity.MeetingLink = meetingLink;
                await _repo.SaveChangesAsync();

                results.Add(new ScheduleResultDto(
                    hrEntity.CandidateRoundID,
                    dto.CandidateId,
                    hrStart.ToUniversalTime(),
                    hrEnd.ToUniversalTime(),
                    meetingLink
                ));
            }


            return results;
        }


        public async Task<CandidateInterviewRound> ScheduleSingleRoundAsync(RoundCreateDto roundDto, int candidateId, int jobId, string recruiterId, bool saveChanges = true)
        {
            if (roundDto == null) throw new ArgumentNullException(nameof(roundDto));
            if (!roundDto.ScheduledDate.HasValue || !roundDto.StartTime.HasValue) throw new InvalidOperationException("Round must have ScheduledDate and StartTime.");

            var scheduledStart = roundDto.ScheduledDate.Value.Date + roundDto.StartTime.Value;
            var scheduledEnd = scheduledStart.AddMinutes(roundDto.DurationMinutes);

            ValidateDateRules(scheduledStart);

            if (!await _repo.IsCandidateAvailableAsync(candidateId, scheduledStart, scheduledEnd))
                throw new InvalidOperationException($"Candidate not available at {scheduledStart}.");

            var candCount = await _repo.CountCandidateInterviewsOnDateAsync(candidateId, scheduledStart.Date);
            if (candCount >= MaxInterviewsPerDay)
                throw new InvalidOperationException($"Candidate already has {MaxInterviewsPerDay} interviews on {scheduledStart.Date:d}");

            if (roundDto.IsPanelRound)
            {
                foreach (var interviewerId in roundDto.InterviewerIds.Distinct())
                {
                    var count = await _repo.CountInterviewerInterviewsOnDateAsync(interviewerId, scheduledStart.Date);
                    if (count >= MaxInterviewsPerDay)
                        throw new InvalidOperationException($"Interviewer {interviewerId} already has {MaxInterviewsPerDay} interviews on {scheduledStart.Date:d}");

                    var startWithBeforeBuffer = scheduledStart - BreakGap;
                    var endWithAfterBuffer = scheduledEnd + BreakGap;
                    if (!await _repo.IsInterviewerAvailableAsync(interviewerId, startWithBeforeBuffer, endWithAfterBuffer))
                        throw new InvalidOperationException($"Interviewer {interviewerId} is not available for panel round at {scheduledStart}.");
                }
            }
            else
            {
                var interviewerId = roundDto.InterviewerIds?.FirstOrDefault()
                    ?? throw new InvalidOperationException("Single interviewer round must contain one interviewer.");
                var count = await _repo.CountInterviewerInterviewsOnDateAsync(interviewerId, scheduledStart.Date);
                if (count >= MaxInterviewsPerDay)
                    throw new InvalidOperationException($"Interviewer {interviewerId} already has {MaxInterviewsPerDay} interviews on {scheduledStart.Date:d}");

                var startWithBeforeBuffer = scheduledStart - BreakGap;
                var endWithAfterBuffer = scheduledEnd + BreakGap;
                if (!await _repo.IsInterviewerAvailableAsync(interviewerId, startWithBeforeBuffer, endWithAfterBuffer))
                    throw new InvalidOperationException($"Interviewer {interviewerId} is not available for round at {scheduledStart}.");
            }

            var roundEntity = new CandidateInterviewRound
            {
                CandidateID = candidateId,
                JobID = jobId,
                SequenceNo = roundDto.SequenceNo,
                RoundName = roundDto.RoundName,
                RoundType = "Technical", 
                IsPanelRound = roundDto.IsPanelRound,
                RecruiterID = recruiterId,
                ScheduledDate = roundDto.ScheduledDate.Value.Date,
                StartTime = roundDto.StartTime.Value,
                DurationMinutes = roundDto.DurationMinutes,
                InterviewMode = roundDto.InterviewMode,
                Status = "Scheduled",
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            await _context.CandidateInterviewRounds.AddAsync(roundEntity);
            if (saveChanges) await _context.SaveChangesAsync(); 

            if (roundDto.IsPanelRound)
            {
                var members = roundDto.InterviewerIds.Distinct()
                    .Select(id => new CandidatePanelMember { CandidateRoundID = roundEntity.CandidateRoundID, InterviewerID = id })
                    .ToList();
                if (members.Any())
                {
                    await _context.CandidatePanelMembers.AddRangeAsync(members);
                    if (saveChanges) await _context.SaveChangesAsync();
                }
            }
            else
            {
                roundEntity.InterviewerID = roundDto.InterviewerIds.FirstOrDefault();
                if (saveChanges) await _context.SaveChangesAsync();
            }

            var meetingLink = GenerateJitsiLink(jobId, candidateId, roundEntity.CandidateRoundID);
            roundEntity.MeetingLink = meetingLink;
            if (saveChanges) await _context.SaveChangesAsync();

            return roundEntity;
        }

        private void ValidateDateRules(DateTime scheduledStart)
        {
            var now = DateTime.UtcNow;
            // 24 hours min
            if (scheduledStart.ToUniversalTime() < now.AddHours(24))
                throw new InvalidOperationException("Interviews must be scheduled at least 24 hours in advance.");

            // weekends
            if (scheduledStart.DayOfWeek == DayOfWeek.Saturday || scheduledStart.DayOfWeek == DayOfWeek.Sunday)
                throw new InvalidOperationException("Cannot schedule interviews on weekends.");

            // 30 day limit
            if (scheduledStart.Date > now.Date.AddDays(30))
                throw new InvalidOperationException("Cannot schedule more than 30 days in advance.");
        }

        private string GenerateJitsiLink(int jobId, int candidateId, int roundId)
        {
            var token = Guid.NewGuid().ToString("n").Substring(0, 8);
            return $"https://meet.jit.si/hyre-job{jobId}-cand{candidateId}-r{roundId}-{token}";
        }
    }
}
