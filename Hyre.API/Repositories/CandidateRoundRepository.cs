using Hyre.API.Data;
using Hyre.API.Interfaces.Scheduling;
using Hyre.API.Models;
using Microsoft.EntityFrameworkCore;

namespace Hyre.API.Repositories
{
    public class CandidateRoundRepository : ICandidateRoundRepository
    {
        private readonly ApplicationDbContext _context;
        public CandidateRoundRepository(ApplicationDbContext context) => _context = context;

        public async Task<List<CandidateInterviewRound>> GetByCandidateAndJobAsync(int candidateId, int jobId)
        {
            return await _context.CandidateInterviewRounds
                .Include(r => r.PanelMembers)
                .Where(r => r.CandidateID == candidateId && r.JobID == jobId)
                .OrderBy(r => r.SequenceNo)
                .ToListAsync();
        }

        public async Task RemoveRoundsByIdsAsync(IEnumerable<int> ids)
        {
            if (ids == null || !ids.Any()) return;
            var rounds = await _context.CandidateInterviewRounds.Where(r => ids.Contains(r.CandidateRoundID)).ToListAsync();
            if (rounds.Any()) _context.CandidateInterviewRounds.RemoveRange(rounds);
        }

        public async Task AddRoundsAsync(IEnumerable<CandidateInterviewRound> rounds)
        {
            if (rounds == null) return;
            await _context.CandidateInterviewRounds.AddRangeAsync(rounds);
        }

        public async Task SaveChangesAsync() => await _context.SaveChangesAsync();
    }
}
