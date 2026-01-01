using Hyre.API.Models;

namespace Hyre.API.Interfaces.InterviewTab
{
    public interface IInterviewRepository
    {
        Task<List<CandidateInterviewRound>> GetRoundsForInterviewerAsync(string interviewerId);
    }
}
