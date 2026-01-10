using Hyre.API.Dtos.Feedback;
using Hyre.API.Dtos.Scheduling;

namespace Hyre.API.Interfaces.Scheduling
{
    public interface ICandidateRoundService
    {
        Task<List<CandidateRoundDto>> GetCandidateRoundsAsync(int candidateId, int jobId);
        Task<UpsertRoundResponseDto> UpsertCandidateRoundsAsync(CandidateRoundsUpdateDto dto, string recruiterId);
        Task<List<JobScheduleStateDto>> GetJobsWithSchedulingStateAsync();
        Task<List<InterviewedCandidateDto>> GetCandidatesBySchedulingStatusAsync(int jobId, string status);
    }
}
