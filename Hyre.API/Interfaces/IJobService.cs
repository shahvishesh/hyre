using Hyre.API.Dtos;

namespace Hyre.API.Interfaces
{
    public interface IJobService
    {
        Task<JobResponseDto> CreateJobAsync(CreateJobDto dto, int createdByUserId);
        Task<List<JobResponseDto>> GetAllJobsAsync();
        Task<JobResponseDto?> GetJobByIdAsync(int jobId);

        Task<JobResponseDto?> UpdateJobAsync(int jobId, UpdateJobDto dto);
        Task<bool> DeleteJobAsync(int jobId);
    }
}
