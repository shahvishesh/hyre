using Hyre.API.Models;

namespace Hyre.API.Interfaces.InterviewerJob
{
    public interface IJobInterviewerRepository
    {
        Task<bool> AnyForJobAsync(int jobId);
        Task<bool> ExistsAsync(int jobId, string interviewerId);
        Task<List<JobInterviewer>> GetAssignedAsync(int jobId);

        Task<List<JobInterviewer>> GetAssignedByRoleAsync(int jobId, string role);
        Task AddAsync(JobInterviewer entity);
        Task RemoveAsync(int jobId, string interviewerId);
        Task<List<Job>> GetJobsByInterviewerStatusAsync(string status);
    }
}
