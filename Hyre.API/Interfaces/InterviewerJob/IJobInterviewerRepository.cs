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
        Task UpdateInterviewerRoleAsync(int jobId, string interviewerId, string newRole);
        Task<List<Job>> GetJobsByInterviewerStatusAsync(string status);
        Task<List<Employee>> GetEmployeesByRoleAsync(string roleName);
        Task<Employee?> GetEmployeeByUserIdAsync(string userId);
        Task<JobInterviewer?> GetInactiveAssignmentAsync(int jobId, string interviewerId);
        Task ReactivateAssignmentAsync(int jobId, string interviewerId, string newRole, string assignedBy);
    }
}
