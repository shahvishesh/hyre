using Hyre.API.Data;
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
    }
}
