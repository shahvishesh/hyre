using Hyre.API.Data;
using Hyre.API.Dtos.Feedback;
using Hyre.API.Interfaces.CandidateFeedback;
using Hyre.API.Models;
using Microsoft.EntityFrameworkCore;

namespace Hyre.API.Services
{
    public class InterviewFeedbackService : IInterviewFeedbackService
    {
        private readonly IInterviewFeedbackRepository _repo;
        private readonly ApplicationDbContext _context;

        public InterviewFeedbackService(
            IInterviewFeedbackRepository repo,
            ApplicationDbContext context)
        {
            _repo = repo;
            _context = context;
        }

        public async Task SubmitFeedbackAsync(
            SubmitFeedbackDto dto,
            string interviewerId)
        {
            var round = await _context.CandidateInterviewRounds
                .FirstOrDefaultAsync(r => r.CandidateRoundID == dto.CandidateRoundID)
                ?? throw new Exception("Interview round not found.");

            if (round.Status != "Completed")
                throw new InvalidOperationException(
                    "Feedback can be submitted only after interview is completed.");

            if (!await _repo.HasAccessToRoundAsync(dto.CandidateRoundID, interviewerId))
                throw new UnauthorizedAccessException("Not authorized for this round.");

            if (await _repo.FeedbackExistsAsync(dto.CandidateRoundID, interviewerId))
                throw new InvalidOperationException("Feedback already submitted.");

            var feedback = new CandidateInterviewFeedback
            {
                CandidateRoundID = dto.CandidateRoundID,
                InterviewerID = interviewerId,
                OverallComment = dto.OverallComment
            };

            foreach (var rating in dto.SkillRatings)
            {
                feedback.SkillRatings.Add(new InterviewSkillRating
                {
                    SkillID = rating.SkillID,
                    Rating = rating.Rating
                });
            }

            await _repo.AddFeedbackAsync(feedback);
            await _repo.SaveAsync();
        }

        public async Task<List<FeedbackResponseDto>> GetMyFeedbacksAsync(string interviewerId)
        {
            var feedbacks = await _repo.GetFeedbacksByInterviewerAsync(interviewerId);

            return feedbacks.Select(MapToDto).ToList();
        }

        public async Task<List<FeedbackResponseDto>> GetFeedbacksForRoundAsync(int roundId)
        {
            var feedbacks = await _repo.GetFeedbacksByRoundAsync(roundId);

            return feedbacks.Select(MapToDto).ToList();
        }

        private static FeedbackResponseDto MapToDto(CandidateInterviewFeedback f)
        {
            return new FeedbackResponseDto(
                f.FeedbackID,
                f.CandidateRoundID,
                $"{f.Interviewer.FirstName} {f.Interviewer.LastName}",
                f.OverallComment,
                f.SubmittedAt,
                f.SkillRatings.Select(sr =>
                    new SkillRatingDto(
                        sr.SkillID,
                        sr.Rating
                    )).ToList()
            );
        }

        public async Task<List<PendingFeedbackDto>> GetPendingFeedbackAsync(string interviewerId)
        {
            var completedRounds = await _repo.GetCompletedRoundsForInterviewerAsync(interviewerId);
            var feedbacks = await _repo.GetFeedbacksByInterviewerAsync(interviewerId);

            var feedbackRoundIds = feedbacks
                .Select(f => f.CandidateRoundID)
                .ToHashSet();

            var pending = completedRounds
                .Where(r => !feedbackRoundIds.Contains(r.CandidateRoundID))
                .Select(r =>
                {
                    var interviewDate =
                        r.ScheduledDate!.Value.Date + r.StartTime!.Value;

                    return new PendingFeedbackDto(
                        r.CandidateRoundID,
                        r.CandidateID,
                        $"{r.Candidate.FirstName} {r.Candidate.LastName}",
                        r.JobID,
                        r.Job.Title,
                        r.RoundName,
                        r.RoundType,
                        interviewDate
                    );
                })
                .OrderBy(p => p.InterviewDate)
                .ToList();

            return pending;
        }

        public async Task<List<FeedbackResponseDto>> GetCompletedFeedbackAsync(string interviewerId)
        {
            var feedbacks = await _repo.GetFeedbacksByInterviewerAsync(interviewerId);

            return feedbacks
                .OrderByDescending(f => f.SubmittedAt)
                .Select(f => new FeedbackResponseDto(
                    f.FeedbackID,
                    f.CandidateRoundID,
                    $"{f.Interviewer.FirstName} {f.Interviewer.LastName}",
                    f.OverallComment,
                    f.SubmittedAt,
                    f.SkillRatings.Select(sr =>
                        new SkillRatingDto(sr.SkillID, sr.Rating)).ToList()
                ))
                .ToList();
        }

        public async Task<List<InterviewerJobDto>> GetInterviewerJobsAsync(string interviewerId)
        {
            var jobs = await _repo.GetJobsForInterviewerAsync(interviewerId);
            
            var result = new List<InterviewerJobDto>();

            foreach (var job in jobs)
            {
                

                result.Add(new InterviewerJobDto(
                    job.JobID,
                    job.Title,
                    job.Description,
                    job.CompanyName,
                    job.Location,
                    job.JobType,
                    job.WorkplaceType,
                    job.Status,
                    job.MinExperience,
                    job.MaxExperience,
                    job.CreatedAt
                ));
            }

            return result;
        }

        public async Task<List<InterviewedCandidateDto>> GetInterviewedCandidatesForJobAsync(int jobId, string interviewerId)
        {
            var hasAccess = await _context.CandidateInterviewRounds
                .AnyAsync(r => r.JobID == jobId &&
                              (r.InterviewerID == interviewerId ||
                               r.PanelMembers.Any(pm => pm.InterviewerID == interviewerId)));

            if (!hasAccess)
                throw new UnauthorizedAccessException("Not authorized to view candidates for this job.");

            var candidates = await _repo.GetInterviewedCandidatesForJobAsync(jobId, interviewerId);

            return candidates.Select(c => new InterviewedCandidateDto(
                c.CandidateID,
                c.FirstName,
                c.LastName,
                c.Email,
                c.Phone,
                c.ExperienceYears,
                c.ResumePath,
                c.Status,
                c.CandidateSkills?
                    .Where(cs => cs.Skill != null)
                    .Select(cs => new CandidateSkillDto(
                        cs.SkillID,
                        cs.Skill.SkillName,
                        cs.YearsOfExperience
                    ))
                    .ToList() ?? new List<CandidateSkillDto>()
            )).ToList();
        }

        public async Task<List<PendingFeedbackDto>> GetPendingFeedbackForCandidateJobAsync(int candidateId, int jobId, string interviewerId)
        {
            // Verify interviewer has access to this candidate for this specific job
            var hasAccess = await _context.CandidateInterviewRounds
                .AnyAsync(r => r.CandidateID == candidateId &&
                              r.JobID == jobId &&
                              (r.InterviewerID == interviewerId ||
                               r.PanelMembers.Any(pm => pm.InterviewerID == interviewerId)));

            if (!hasAccess)
                throw new UnauthorizedAccessException("Not authorized to view feedback for this candidate and job combination.");

            var completedRounds = await _repo.GetCompletedRoundsForCandidateJobAsync(candidateId, jobId, interviewerId);
            var feedbacks = await _repo.GetFeedbacksByInterviewerAsync(interviewerId);

            var feedbackRoundIds = feedbacks
                .Select(f => f.CandidateRoundID)
                .ToHashSet();

            var pending = completedRounds
                .Where(r => !feedbackRoundIds.Contains(r.CandidateRoundID))
                .Select(r =>
                {
                    var interviewDate = r.ScheduledDate!.Value.Date + r.StartTime!.Value;

                    return new PendingFeedbackDto(
                        r.CandidateRoundID,
                        r.CandidateID,
                        $"{r.Candidate.FirstName} {r.Candidate.LastName}",
                        r.JobID,
                        r.Job.Title,
                        r.RoundName,
                        r.RoundType,
                        interviewDate
                    );
                })
                .OrderBy(p => p.InterviewDate)
                .ToList();

            return pending;
        }

        public async Task<List<CompletedFeedbackDto>> GetCompletedFeedbackForCandidateJobAsync(int candidateId, int jobId, string interviewerId)
        {
            // Verify interviewer has access to this candidate for this specific job
            var hasAccess = await _context.CandidateInterviewRounds
                .AnyAsync(r => r.CandidateID == candidateId &&
                              r.JobID == jobId &&
                              (r.InterviewerID == interviewerId ||
                               r.PanelMembers.Any(pm => pm.InterviewerID == interviewerId)));

            if (!hasAccess)
                throw new UnauthorizedAccessException("Not authorized to view feedback for this candidate and job combination.");

            var completedRoundsWithFeedback = await _repo.GetCompletedRoundsWithFeedbackForCandidateJobAsync(candidateId, jobId, interviewerId);

            var completed = completedRoundsWithFeedback
                .Select(r =>
                {
                    var interviewDate = r.ScheduledDate!.Value.Date + r.StartTime!.Value;

                    return new CompletedFeedbackDto(
                        r.CandidateRoundID,
                        r.CandidateID,
                        $"{r.Candidate.FirstName} {r.Candidate.LastName}",
                        r.JobID,
                        r.Job.Title,
                        r.RoundName,
                        r.RoundType,
                        interviewDate
                    );
                })
                .OrderBy(c => c.InterviewDate)
                .ToList();

            return completed;
        }

        public async Task<RoundDetailDto> GetRoundDetailAsync(int candidateRoundId, string interviewerId)
        {
            var round = await _repo.GetRoundByIdAsync(candidateRoundId)
                ?? throw new ArgumentException("Round not found.", nameof(candidateRoundId));

            // Verify interviewer has access to this round
            if (!await _repo.HasAccessToRoundAsync(candidateRoundId, interviewerId))
                throw new UnauthorizedAccessException("Not authorized to view this round.");

            // Build panel members list (only if it's a panel round)
            List<PanelMemberDetailDto>? panelMembers = null;
            if (round.IsPanelRound && round.PanelMembers != null && round.PanelMembers.Any())
            {
                panelMembers = round.PanelMembers
                    .Where(pm => pm.Interviewer != null)
                    .Select(pm => new PanelMemberDetailDto(
                        pm.InterviewerID,
                        pm.Interviewer.FirstName,
                        pm.Interviewer.LastName,
                        pm.Interviewer.Email
                    ))
                    .ToList();
            }

            // Build interviewer details (only for non-panel rounds)
            InterviewerDetailDto? interviewer = null;
            if (!round.IsPanelRound && round.Interviewer != null)
            {
                interviewer = new InterviewerDetailDto(
                    round.InterviewerID!,
                    round.Interviewer.FirstName,
                    round.Interviewer.LastName,
                    round.Interviewer.Email
                );
            }

            return new RoundDetailDto(
                round.CandidateRoundID,
                round.CandidateID,
                $"{round.Candidate.FirstName} {round.Candidate.LastName}",
                round.JobID,
                round.Job.Title,
                round.SequenceNo,
                round.RoundName,
                round.RoundType,
                round.IsPanelRound,
                round.ScheduledDate,
                round.StartTime,
                round.DurationMinutes,
                round.InterviewMode,
                round.Status,
                round.MeetingLink,
                round.CreatedAt,
                round.UpdatedAt,
                round.RecruiterDecision,
                round.RecruiterDecisionAt,
                panelMembers,
                interviewer
            );
        }

        public async Task<SimpleFeedbackDto> GetMyFeedbackForRoundAsync(int candidateRoundId, string interviewerId)
        {
            if (!await _repo.HasAccessToRoundAsync(candidateRoundId, interviewerId))
                throw new UnauthorizedAccessException("Not authorized to view this round.");

            var feedback = await _repo.GetFeedbackByRoundAndInterviewerAsync(candidateRoundId, interviewerId)
                ?? throw new InvalidOperationException("Feedback not found for this round and interviewer combination.");

            return new SimpleFeedbackDto(
                feedback.CandidateRoundID,
                feedback.OverallComment,
                feedback.SkillRatings.Select(sr => new SkillRatingDto(
                    sr.SkillID,
                    sr.Rating
                )).ToList()
            );
        }
    }
}
