using Hyre.API.Dtos.InterviewerJob;

namespace Hyre.API.Interfaces.InterviewerJob
{
    public interface IJobInterviewerService
    {
        Task AssignInterviewersAsync(AssignInterviewersDto dto, string recruiterId);
        Task RemoveInterviewerAsync(int jobId, string interviewerId);
        Task<List<JobInterviewerDto>> GetAssignedInterviewersAsync(int jobId);

        Task<List<JobInterviewerDto>> GetInterviewersByRoleAsync(int jobId, string role);
        Task<List<InterviewerJobResponseDto>> GetJobsByInterviewerStatusAsync(string status);
        Task<List<EmployeeDetailDto>> GetEmployeesBySystemRoleAsync(string role);
    }
}
