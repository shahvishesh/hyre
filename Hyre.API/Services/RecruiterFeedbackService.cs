using Hyre.API.Dtos.Feedback;
using Hyre.API.Dtos.RecruiterRoundDecesion;
using Hyre.API.Interfaces.RecruiterFeedback;

namespace Hyre.API.Services
{
    public class RecruiterFeedbackService : IRecruiterFeedbackService
    {
        private readonly IRecruiterFeedbackRepository _repo;

        public RecruiterFeedbackService(IRecruiterFeedbackRepository repo)
        {
            _repo = repo;
        }

        public async Task<RoundAggregatedFeedbackDto> GetAggregatedFeedbackAsync(int roundId)
        {
            var round = await _repo.GetRoundWithFeedbackAsync(roundId)
                ?? throw new Exception("Round not found.");

            var feedbacks = round.Feedbacks.ToList();

            var allSkillRatings = feedbacks
                .SelectMany(f => f.SkillRatings)
                .GroupBy(sr => sr.SkillID);

            var skillAggregates = allSkillRatings
                .Select(g => new SkillAggregateDto(
                    g.Key,
                    g.First().Skill.SkillName,
                    Math.Round(g.Average(x => x.Rating), 2),
                    g.Count()
                ))
                .ToList();

            var individual = feedbacks.Select(f =>
                new InterviewerFeedbackDto(
                    f.InterviewerID,
                    $"{f.Interviewer.FirstName} {f.Interviewer.LastName}",
                    f.OverallComment,
                    f.SubmittedAt,
                    f.SkillRatings.Select(sr =>
                        new SkillRatingDto(sr.SkillID, sr.Rating)).ToList()
                )).ToList();

            int totalInterviewers = round.IsPanelRound
                ? round.PanelMembers.Count
                : 1;

            return new RoundAggregatedFeedbackDto(
                round.CandidateRoundID,
                round.RoundName,
                round.RoundType,
                round.Status,
                totalInterviewers,
                feedbacks.Count,
                skillAggregates,
                individual
            );
        }
    }
}