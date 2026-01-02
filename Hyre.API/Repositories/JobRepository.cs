using Hyre.API.Data;
using Hyre.API.Interfaces;
using Hyre.API.Models;
using Microsoft.EntityFrameworkCore;

namespace Hyre.API.Repositories
{
    public class JobRepository : IJobRepository
    {
        private readonly ApplicationDbContext _context;

        public JobRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<Job> AddAsync(Job job)
        {
            _context.Jobs.Add(job);
            await _context.SaveChangesAsync();
            return job;
        }

        public async Task<Job?> GetByIdAsync(int jobId)
        {
            return await _context.Jobs
                .Include(j => j.Creator)
                .Include(j => j.SelectedCandidate)
                .Include(j => j.JobSkills)
                .ThenInclude(js => js.Skill)
                .Include(j => j.InterviewRoundTemplates)
                .FirstOrDefaultAsync(j => j.JobID == jobId);
        }

        public async Task<IEnumerable<Job>> GetAllAsync()
        {
            return await _context.Jobs
                .Include(j => j.Creator)
                .Include(j => j.SelectedCandidate)
                .Include(j => j.JobSkills)
                .ThenInclude(js => js.Skill)
                .Include(j => j.InterviewRoundTemplates)
                .ToListAsync();
        }

        public async Task UpdateAsync(Job job)
        {
            _context.Jobs.Update(job);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(int jobId)
        {
            var job = await _context.Jobs.FindAsync(jobId);
            if (job != null)
            {
                _context.Jobs.Remove(job);
                await _context.SaveChangesAsync();
            }
        }
    }
}
