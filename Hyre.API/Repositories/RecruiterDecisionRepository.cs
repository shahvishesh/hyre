using Hyre.API.Data;
using Hyre.API.Interfaces.RecruiterFeedback;
using Hyre.API.Models;
using Microsoft.EntityFrameworkCore;

namespace Hyre.API.Repositories
{
    public class RecruiterDecisionRepository : IRecruiterDecisionRepository
    {
        private readonly ApplicationDbContext _context;

        public RecruiterDecisionRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<CandidateInterviewRound?> GetRoundAsync(int roundId)
        {
            return await _context.CandidateInterviewRounds
                .FirstOrDefaultAsync(r => r.CandidateRoundID == roundId);
        }

        public async Task<CandidateInterviewRound?> GetNextRoundAsync(
            int candidateId, int jobId, int currentSequenceNo)
        {
            return await _context.CandidateInterviewRounds
                .Where(r =>
                    r.CandidateID == candidateId &&
                    r.JobID == jobId &&
                    r.SequenceNo == currentSequenceNo + 1)
                .FirstOrDefaultAsync();
        }

        public async Task<List<CandidateInterviewRound>> GetFutureRoundsAsync(
            int candidateId, int jobId, int fromSequenceNo)
        {
            return await _context.CandidateInterviewRounds
                .Where(r =>
                    r.CandidateID == candidateId &&
                    r.JobID == jobId &&
                    r.SequenceNo > fromSequenceNo)
                .ToListAsync();
        }

        public async Task SaveAsync()
        {
            await _context.SaveChangesAsync();
        }
    }
}
