using Hyre.API.Data;
using Hyre.API.Dtos.RecruiterRoundDecesion;
using Hyre.API.Interfaces.RecruiterFeedback;
using Hyre.API.Models;
using Microsoft.EntityFrameworkCore;

namespace Hyre.API.Repositories
{
    public class RecruiterFeedbackRepository : IRecruiterFeedbackRepository
    {
        private readonly ApplicationDbContext _context;

        public RecruiterFeedbackRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<CandidateInterviewRound?> GetRoundWithFeedbackAsync(int roundId)
        {
            return await _context.CandidateInterviewRounds
                .Include(r => r.PanelMembers)
                    .ThenInclude(pm => pm.Interviewer)
                .Include(r => r.Interviewer)
                .Include(r => r.Feedbacks)
                    .ThenInclude(f => f.Interviewer)
                .Include(r => r.Feedbacks)
                    .ThenInclude(f => f.SkillRatings)
                        .ThenInclude(sr => sr.Skill)
                .FirstOrDefaultAsync(r => r.CandidateRoundID == roundId);
        }


        public async Task<IEnumerable<CandidateInterviewRound>> GetRoundsByDecisionStateAsync(int candidateId, int jobId, RecruiterDecisionState decisionState)
        {
            var query = _context.CandidateInterviewRounds
                .Include(r => r.Candidate)
                .Include(r => r.Job)
                .Where(r => r.CandidateID == candidateId && 
                           r.JobID == jobId && 
                           r.Status == "Completed");

            query = decisionState switch
            {
                RecruiterDecisionState.Pending => query.Where(r => r.RecruiterDecision == null),
                RecruiterDecisionState.Decided => query.Where(r => r.RecruiterDecision != null),
                RecruiterDecisionState.All => query,
                _ => throw new ArgumentException($"Invalid decision state: {decisionState}")
            };

            return await query.OrderBy(r => r.SequenceNo).ToListAsync();
        }

        public async Task<IEnumerable<CandidateInterviewRound>> GetExpiredRoundsAsync(int candidateId, int jobId)
        {
            return await _context.CandidateInterviewRounds
                .Include(r => r.Candidate)
                .Include(r => r.Job)
                .Where(r => r.CandidateID == candidateId && 
                           r.JobID == jobId && 
                           r.Status == "Expired")
                .OrderBy(r => r.SequenceNo)
                .ToListAsync();
        }

        public async Task<List<Candidate>> GetInterviewedCandidatesForJobAsync(int jobId)
        {
            return await _context.CandidateInterviewRounds
                .Include(r => r.Candidate)
                    .ThenInclude(c => c.CandidateSkills)
                        .ThenInclude(cs => cs.Skill)
                .Where(r => r.JobID == jobId)
                .Select(r => r.Candidate)
                .Distinct()
                .OrderBy(c => c.FirstName)
                .ThenBy(c => c.LastName)
                .ToListAsync();
        }

        public async Task<CandidateInterviewRound?> GetRoundByIdAsync(int candidateRoundId)
        {
            return await _context.CandidateInterviewRounds
                .Include(r => r.Candidate)
                .Include(r => r.Job)
                .Include(r => r.PanelMembers)
                    .ThenInclude(pm => pm.Interviewer)
                .Include(r => r.Interviewer)
                .FirstOrDefaultAsync(r => r.CandidateRoundID == candidateRoundId);
        }

    }
}
