using Hyre.API.Models;

namespace Hyre.API.Interfaces.Scheduling
{
    public interface ICandidateInterviewRepository
    {
        Task AddCandidateInterviewRoundAsync(CandidateInterviewRound round);
        Task AddCandidatePanelMembersAsync(IEnumerable<CandidatePanelMember> members);
        Task<bool> IsInterviewerAvailableAsync(string interviewerId, DateTime startUtc, DateTime endUtc);
        Task<bool> IsCandidateAvailableAsync(int candidateId, DateTime startUtc, DateTime endUtc);
        Task<int> CountInterviewerInterviewsOnDateAsync(string interviewerId, DateTime date);
        Task<int> CountCandidateInterviewsOnDateAsync(int candidateId, DateTime date);
        Task<CandidateInterviewRound?> GetRoundByIdAsync(int id);
        Task SaveChangesAsync();
    }
}
