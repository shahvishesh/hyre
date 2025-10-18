using Hyre.API.Data;
using Hyre.API.Interfaces.Candidates;
using Hyre.API.Models;
using Microsoft.EntityFrameworkCore;

namespace Hyre.API.Repositories
{
    public class CandidateRepository : ICandidateRepository
    {
        private readonly ApplicationDbContext _context;

        public CandidateRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task AddCandidateAsync(Candidate candidate)
        {
            _context.Candidates.Add(candidate);
            await _context.SaveChangesAsync();
        }

        public async Task AddCandidatesAsync(IEnumerable<Candidate> candidates)
        {
            _context.Candidates.AddRange(candidates);
            await _context.SaveChangesAsync();
        }

        public async Task<Candidate?> GetCandidateByIdAsync(int candidateId)
        {
            return await _context.Candidates
                .Include(c => c.CandidateSkills)
                .ThenInclude(cs => cs.Skill)
                .FirstOrDefaultAsync(c => c.CandidateID == candidateId);
        }

        public async Task UpdateResumePathAsync(int candidateId, string resumePath)
        {
            var candidate = await _context.Candidates.FindAsync(candidateId);
            if (candidate != null)
            {
                candidate.ResumePath = resumePath;
                candidate.UpdatedAt = DateTime.Now;
                await _context.SaveChangesAsync();
            }
        }
    }
}