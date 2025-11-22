namespace Hyre.API.Interfaces.Scheduling
{
    public interface IInterviewScheduleRepository
    {
        Task<List<(DateTime Start, DateTime End)>> GetBusyIntervalsForInterviewerAsync(string interviewerId, DateTime date);
        Task<int> CountInterviewerInterviewsOnDateAsync(string interviewerId, DateTime date);
        Task<int> CountCandidateInterviewsOnDateAsync(int candidateId, DateTime date);
        Task<List<(DateTime Start, DateTime End)>> GetBusyIntervalsForCandidateAsync(int candidateId, DateTime date);
    }
}
