using Hyre.API.Dtos.Interviews;
using Hyre.API.Enums;
using Hyre.API.Interfaces.InterviewTab;

namespace Hyre.API.Services
{
    public class InterviewService : IInterviewService
    {
        private readonly IInterviewRepository _repo;
        private static readonly TimeSpan PreStartGrace = TimeSpan.FromMinutes(5);
        private static readonly TimeSpan PostEndGrace = TimeSpan.FromMinutes(10);


        public InterviewService(IInterviewRepository repo)
        {
            _repo = repo;
        }

        public async Task<List<InterviewRoundDto>> GetRoundsByTabAsync(
            string interviewerId,
            InterviewTabs tab)
        {
            var rounds = await _repo.GetRoundsForInterviewerAsync(interviewerId);
            var now = DateTime.UtcNow;

            var filtered = rounds.Where(r =>
            {
                if (!r.ScheduledDate.HasValue || !r.StartTime.HasValue || !r.DurationMinutes.HasValue)
                    return false;

                var start = r.ScheduledDate.Value.Date + r.StartTime.Value;
                var end = start.AddMinutes(r.DurationMinutes.Value);

                var liveStart = start - PreStartGrace;
                var liveEnd = end + PostEndGrace;

                return tab switch
                {
                    InterviewTabs.Live =>
                        now >= liveStart && now <= liveEnd && r.Status == "Scheduled",

                    InterviewTabs.Today =>
                        start.Date == now.Date && now < liveStart,

                    InterviewTabs.Upcoming =>
                        start.Date > now.Date,

                    InterviewTabs.Completed =>
                        r.Status == "Completed" || (now > liveEnd && r.Status == "Scheduled"),

                    InterviewTabs.Expired =>
                        r.Status == "Expired",

                    _ => false
                };

            });

            return filtered
                .OrderBy(r => r.ScheduledDate)
                .ThenBy(r => r.StartTime)
                .Select(r =>
                {
                    var start = r.ScheduledDate!.Value.Date + r.StartTime!.Value;
                    var end = start.AddMinutes(r.DurationMinutes!.Value);

                    return new InterviewRoundDto(
                        r.CandidateRoundID,
                        r.CandidateID,
                        $"{r.Candidate.FirstName} {r.Candidate.LastName}",
                        r.JobID,
                        r.Job.Title,
                        r.RoundName,
                        r.RoundType,
                        r.IsPanelRound,
                        start,
                        end,
                        r.Status,
                        r.InterviewMode,
                        r.MeetingLink
                    );
                })
                .ToList();
        }
    }
}