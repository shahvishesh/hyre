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

    }
}
