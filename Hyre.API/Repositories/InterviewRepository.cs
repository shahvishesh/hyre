using Hyre.API.Data;
using Hyre.API.Interfaces.InterviewTab;
using Hyre.API.Models;
using Microsoft.EntityFrameworkCore;

namespace Hyre.API.Repositories
{
    public class InterviewRepository : IInterviewRepository
    {
        private readonly ApplicationDbContext _context;

        public InterviewRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<List<CandidateInterviewRound>> GetRoundsForInterviewerAsync(string interviewerId)
        {
            return await _context.CandidateInterviewRounds
                .Include(r => r.Candidate)
                    .ThenInclude(c => c.CandidateSkills)
                        .ThenInclude(cs => cs.Skill)
                .Include(r => r.Job)
                    .ThenInclude(j => j.JobSkills)
                        .ThenInclude(js => js.Skill)
                .Include(r => r.PanelMembers)
                    .ThenInclude(pm => pm.Interviewer)
                .Where(r =>
                    r.InterviewerID == interviewerId ||
                    r.PanelMembers.Any(pm => pm.InterviewerID == interviewerId))
                .ToListAsync();
        }
    }
}
