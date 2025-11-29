using Hyre.API.Data;
using Hyre.API.Interfaces;
using Hyre.API.Interfaces.InterviewerJob;
using Hyre.API.Models;
using Microsoft.EntityFrameworkCore;

namespace Hyre.API.Repositories
{
    public class JobInterviewerRepository : IJobInterviewerRepository
    {
        private readonly ApplicationDbContext _context;

        public JobInterviewerRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<bool> AnyForJobAsync(int jobId)
        {
            return await _context.JobInterviewers
                .AnyAsync(x => x.JobID == jobId && x.IsActive);
        }

        public async Task<bool> ExistsAsync(int jobId, string interviewerId)
        {
            return await _context.JobInterviewers
                .AnyAsync(x => x.JobID == jobId &&
                               x.InterviewerID == interviewerId &&
                               x.IsActive);
        }

        public async Task<List<JobInterviewer>> GetAssignedAsync(int jobId)
        {
            return await _context.JobInterviewers
                .Include(x => x.Interviewer)
                .Where(x => x.JobID == jobId && x.IsActive)
                .ToListAsync();
        }

        public async Task<List<JobInterviewer>> GetAssignedByRoleAsync(int jobId, string role)
        {
            return await _context.JobInterviewers
                .Include(x => x.Interviewer)
                .Where(x => x.JobID == jobId && x.Role == role && x.IsActive)
                .ToListAsync();
        }


        public async Task AddAsync(JobInterviewer entity)
        {
            _context.JobInterviewers.Add(entity);
            await _context.SaveChangesAsync();
        }

        public async Task RemoveAsync(int jobId, string interviewerId)
        {
            var entity = await _context.JobInterviewers
                .FirstOrDefaultAsync(x =>
                    x.JobID == jobId &&
                    x.InterviewerID == interviewerId &&
                    x.IsActive);

            if (entity != null)
            {
                entity.IsActive = false;
                await _context.SaveChangesAsync();
            }
        }
    }
}
