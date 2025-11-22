using Hyre.API.Dtos.Scheduling;

namespace Hyre.API.Interfaces.Scheduling
{
    public interface ICandidateInterviewService
    {
        Task<List<ScheduleResultDto>> ScheduleRoundsAsync(CreateCandidateInterviewDto dto, string recruiterId);
    }
}
