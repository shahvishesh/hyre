using Hyre.API.Data;
using Hyre.API.Dtos.Feedback;
using Hyre.API.Dtos.RecruiterRoundDecesion;
using Hyre.API.Interfaces.RecruiterFeedback;
using Microsoft.EntityFrameworkCore;

namespace Hyre.API.Services
{
    public class RecruiterFeedbackService : IRecruiterFeedbackService
    {
        private readonly IRecruiterFeedbackRepository _repo;
        private readonly ApplicationDbContext _context;

        public RecruiterFeedbackService(IRecruiterFeedbackRepository repo, ApplicationDbContext context)
        {
            _repo = repo;
            _context = context;
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

       

        public async Task<IEnumerable<PendingRecruiterDecisionDto>> GetRoundsByDecisionStateAsync(int candidateId, int jobId, RecruiterDecisionState decisionState)
        {
            var rounds = await _repo.GetRoundsByDecisionStateAsync(candidateId, jobId, decisionState);

            return rounds.Select(r => new PendingRecruiterDecisionDto(
                r.CandidateRoundID,
                r.CandidateID,
                $"{r.Candidate.FirstName} {r.Candidate.LastName}",
                r.JobID,
                r.Job.Title,
                r.RoundName,
                r.RoundType ?? string.Empty,
                r.ScheduledDate ?? DateTime.MinValue,
                r.Status
            ));
        }

        public async Task<List<InterviewedCandidateDto>> GetInterviewedCandidatesForJobAsync(int jobId)
        {
            
            var candidates = await _repo.GetInterviewedCandidatesForJobAsync(jobId);

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

        public async Task<RoundDetailDto> GetRoundDetailAsync(int candidateRoundId)
        {
            var round = await _repo.GetRoundByIdAsync(candidateRoundId)
                ?? throw new ArgumentException("Round not found.", nameof(candidateRoundId));

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
    }
}