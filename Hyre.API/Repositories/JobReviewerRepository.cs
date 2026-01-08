using Hyre.API.Data;
using Hyre.API.Interfaces.ReviewerJob;
using Hyre.API.Models;
using Microsoft.EntityFrameworkCore;

namespace Hyre.API.Repositories
{
    public class JobReviewerRepository : IJobReviewerRepository
    {
        private readonly ApplicationDbContext _context;

        public JobReviewerRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        /*public async Task AssignReviewersAsync(int jobId, List<string> reviewerIds, string assignedBy)
        {
            var existing = await _context.JobReviewers
                .Where(jr => jr.JobId == jobId)
                .ToListAsync();

            var newIds = reviewerIds.Except(existing.Select(e => e.ReviewerId)).ToList();

            foreach (var reviewerId in newIds)
            {
                _context.JobReviewers.Add(new JobReviewer
                {
                    JobId = jobId,
                    ReviewerId = reviewerId,
                    AssignedBy = assignedBy,
                    AssignedAt = DateTime.UtcNow
                });
            }

            await _context.SaveChangesAsync();
        }*/
        public async Task AssignReviewersAsync(
    int jobId,
    List<string> reviewerIds,
    string assignedBy)
        {
            var existing = await _context.JobReviewers
                .Where(jr => jr.JobId == jobId)
                .ToListAsync();

            var existingIds = existing.Select(e => e.ReviewerId).ToList();

            // Add new reviewers
            var toAdd = reviewerIds.Except(existingIds).ToList();
            foreach (var reviewerId in toAdd)
            {
                _context.JobReviewers.Add(new JobReviewer
                {
                    JobId = jobId,
                    ReviewerId = reviewerId,
                    AssignedBy = assignedBy,
                    AssignedAt = DateTime.UtcNow
                });
            }

            // Remove unselected reviewers
            var toRemove = existing
                .Where(e => !reviewerIds.Contains(e.ReviewerId))
                .ToList();

            _context.JobReviewers.RemoveRange(toRemove);

            await _context.SaveChangesAsync();
        }


        public async Task<List<JobReviewer>> GetReviewersByJobIdAsync(int jobId)
        {
            return await _context.JobReviewers
                .Include(jr => jr.Reviewer)
                .Where(jr => jr.JobId == jobId)
                .ToListAsync();
        }

        public async Task RemoveReviewerAsync(int jobId, string reviewerId)
        {
            var entry = await _context.JobReviewers
                .FirstOrDefaultAsync(jr => jr.JobId == jobId && jr.ReviewerId == reviewerId);

            if (entry != null)
            {
                _context.JobReviewers.Remove(entry);
                await _context.SaveChangesAsync();
            }
        }

        public async Task<bool> IsReviewerAssignedToJobAsync(string reviewerId, int jobId)
        {
            return await _context.JobReviewers
                .AnyAsync(jr => jr.JobId == jobId && jr.ReviewerId == reviewerId);
        }

        public async Task<List<Job>> GetJobsByReviewerStatusAsync(string status)
        {
            if (status.ToLower() == "pending")
            {
                // Jobs where no reviewers are assigned
                return await _context.Jobs
                    .Include(j => j.JobSkills)
                        .ThenInclude(js => js.Skill)
                    .Where(j => !_context.JobReviewers.Any(jr => jr.JobId == j.JobID))
                    .ToListAsync();
            }
            else if (status.ToLower() == "completed")
            {
                // Jobs where at least one reviewer is assigned
                return await _context.Jobs
                    .Include(j => j.JobSkills)
                        .ThenInclude(js => js.Skill)
                    .Where(j => _context.JobReviewers.Any(jr => jr.JobId == j.JobID))
                    .ToListAsync();
            }
            else
            {
                throw new ArgumentException("Invalid status. Use 'pending' or 'completed'.");
            }
        }
    }

}
