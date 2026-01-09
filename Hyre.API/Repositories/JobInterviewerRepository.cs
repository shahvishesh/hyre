using Hyre.API.Data;
using Hyre.API.Interfaces;
using Hyre.API.Interfaces.InterviewerJob;
using Hyre.API.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Hyre.API.Repositories
{
    public class JobInterviewerRepository : IJobInterviewerRepository
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public JobInterviewerRepository(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
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

        public async Task<List<Job>> GetJobsByInterviewerStatusAsync(string status)
        {
            var query = _context.Jobs
                .Include(j => j.JobSkills)
                    .ThenInclude(js => js.Skill)
                .AsQueryable();

            if (status.ToLower() == "pending")
            {
                query = query.Where(j => !_context.JobInterviewers
                    .Any(ji => ji.JobID == j.JobID && ji.IsActive));
            }
            else if (status.ToLower() == "completed")
            {
                query = query.Where(j => _context.JobInterviewers
                    .Any(ji => ji.JobID == j.JobID && ji.IsActive));
            }

            return await query.ToListAsync();
        }

        public async Task<List<Employee>> GetEmployeesByRoleAsync(string roleName)
        {
            var usersInRole = await _userManager.GetUsersInRoleAsync(roleName);

            var userIds = usersInRole
                .Select(u => u.Id)
                .ToList();

            return await _context.Employees
                .Include(e => e.User)
                .Where(e => e.UserID != null
                 && userIds.Contains(e.UserID)
                 && e.EmploymentStatus == "Active") 
                .ToListAsync();
        }
    }
}
