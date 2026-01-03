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

        public async Task<List<LiveInterviewDetailDto>> GetLiveInterviewsWithDetailsAsync(string interviewerId)
        {
            var rounds = await _repo.GetRoundsForInterviewerAsync(interviewerId);
            var now = DateTime.UtcNow;

            var liveRounds = rounds.Where(r =>
            {
                if (!r.ScheduledDate.HasValue || !r.StartTime.HasValue || !r.DurationMinutes.HasValue)
                    return false;

                var start = r.ScheduledDate.Value.Date + r.StartTime.Value;
                var end = start.AddMinutes(r.DurationMinutes.Value);

                var liveStart = start - PreStartGrace;
                var liveEnd = end + PostEndGrace;

                return now >= liveStart && now <= liveEnd && r.Status == "Scheduled";
            });

            return liveRounds
                .OrderBy(r => r.ScheduledDate)
                .ThenBy(r => r.StartTime)
                .Select(r =>
                {
                    var start = r.ScheduledDate!.Value.Date + r.StartTime!.Value;
                    var end = start.AddMinutes(r.DurationMinutes!.Value);

                    // Build panel members list (only if it's a panel round)
                    List<PanelMemberDto>? panelMembers = null;
                    if (r.IsPanelRound && r.PanelMembers != null && r.PanelMembers.Any())
                    {
                        panelMembers = r.PanelMembers
                            .Where(pm => pm.Interviewer != null)
                            .Select(pm => new PanelMemberDto(
                                pm.InterviewerID,
                                pm.Interviewer.FirstName,
                                pm.Interviewer.LastName,
                                pm.Interviewer.Email
                            ))
                            .ToList();
                    }

                    // Build candidate skills
                    var candidateSkills = r.Candidate.CandidateSkills?
                        .Where(cs => cs.Skill != null)
                        .Select(cs => new CandidateSkillDto(
                            cs.SkillID,
                            cs.Skill.SkillName,
                            cs.YearsOfExperience
                        ))
                        .ToList() ?? new List<CandidateSkillDto>();

                    // Build job skills
                    var jobSkills = r.Job.JobSkills?
                        .Where(js => js.Skill != null)
                        .Select(js => new JobSkillDto(
                            js.SkillID,
                            js.Skill.SkillName,
                            js.SkillType
                        ))
                        .ToList() ?? new List<JobSkillDto>();

                    return new LiveInterviewDetailDto(
                        r.CandidateRoundID,
                        r.RoundName,
                        r.RoundType,
                        r.IsPanelRound,
                        start,
                        end,
                        r.Status,
                        r.InterviewMode,
                        r.MeetingLink,
                        new CandidateDetailDto(
                            r.Candidate.CandidateID,
                            r.Candidate.FirstName,
                            r.Candidate.LastName,
                            r.Candidate.Email,
                            r.Candidate.Phone,
                            r.Candidate.ExperienceYears,
                            r.Candidate.ResumePath,
                            r.Candidate.Status,
                            candidateSkills
                        ),
                        new JobDetailDto(
                            r.Job.JobID,
                            r.Job.Title,
                            r.Job.Description,
                            r.Job.MinExperience,
                            r.Job.MaxExperience,
                            r.Job.CompanyName,
                            r.Job.Location,
                            r.Job.JobType,
                            r.Job.WorkplaceType,
                            r.Job.Status,
                            jobSkills
                        ),
                        panelMembers
                    );
                })
                .ToList();
        }
    }
}