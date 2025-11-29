using Hyre.API.Dtos.Scheduling;
using Hyre.API.Models;

namespace Hyre.API.Interfaces.Scheduling
{
    public interface ICandidateInterviewService
    {
        Task<List<ScheduleResultDto>> ScheduleRoundsAsync(CreateCandidateInterviewDto dto, string recruiterId);

        Task<CandidateInterviewRound> ScheduleSingleRoundAsync(RoundCreateDto roundDto, int candidateId, int jobId, string recruiterId, bool saveChanges = true);
    }
}
