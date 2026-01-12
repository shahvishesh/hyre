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

        public async Task<CandidateInterviewRound?> GetNextRoundDetailAsync(
            int candidateId, int jobId, int currentSequenceNo)
        {
            return await _context.CandidateInterviewRounds
                .Include(r => r.Candidate)
                .Include(r => r.Job)
                .Include(r => r.Interviewer)
                .Include(r => r.PanelMembers)
                    .ThenInclude(pm => pm.Interviewer)
                .Where(r =>
                    r.CandidateID == candidateId &&
                    r.JobID == jobId &&
                    r.SequenceNo == currentSequenceNo + 1)
                .FirstOrDefaultAsync();
        }

        public async Task<CandidateInterviewRound?> GetRoundWithDecisionDetailsAsync(int roundId)
        {
            return await _context.CandidateInterviewRounds
                .Include(r => r.Candidate)
                .Include(r => r.Job)
                .Include(r => r.Recruiter)
                .FirstOrDefaultAsync(r => r.CandidateRoundID == roundId);
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

        public async Task<CandidateJob?> GetCandidateJobAsync(int candidateId, int jobId)
        {
            return await _context.CandidateJobs
                .FirstOrDefaultAsync(cj =>
                    cj.CandidateID == candidateId &&
                    cj.JobID == jobId);
        }

        public async Task<CandidateDocumentVerification?> GetExistingDocumentVerificationAsync(int candidateId, int jobId)
        {
            return await _context.CandidateDocumentVerifications
                .FirstOrDefaultAsync(v =>
                    v.CandidateId == candidateId &&
                    v.JobId == jobId);
        }

        public async Task CreateCandidateDocumentVerificationAsync(CandidateDocumentVerification verification)
        {
            _context.CandidateDocumentVerifications.Add(verification);
        }

        public async Task SaveAsync()
        {
            await _context.SaveChangesAsync();
        }
    }
}
