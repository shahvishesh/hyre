using Hyre.API.Dtos.Interviews;
using Hyre.API.Enums;

namespace Hyre.API.Interfaces.InterviewTab
{
    public interface IInterviewService
    {
        Task<List<InterviewRoundDto>> GetRoundsByTabAsync(string interviewerId, InterviewTabs tab);
        Task<List<LiveInterviewDetailDto>> GetLiveInterviewsWithDetailsAsync(string interviewerId);
    }
}
