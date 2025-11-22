using Hyre.API.Data;
using Hyre.API.Interfaces.Scheduling;
using Hyre.API.Models;
using Microsoft.EntityFrameworkCore;

namespace Hyre.API.Repositories
{
    public class InterviewScheduleRepository : IInterviewScheduleRepository
    {
        private readonly ApplicationDbContext _context;

        public InterviewScheduleRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        private (DateTime Start, DateTime End)? RoundToInterval(CandidateInterviewRound r)
        {
            if (!r.ScheduledDate.HasValue || !r.StartTime.HasValue || !r.DurationMinutes.HasValue) return null;
            var start = r.ScheduledDate.Value.Date + r.StartTime.Value;
            var end = start.AddMinutes(r.DurationMinutes.Value);
            return (start, end);
        }

        public async Task<List<(DateTime Start, DateTime End)>> GetBusyIntervalsForInterviewerAsync(string interviewerId, DateTime date)
        {
            date = date.Date;
            var result = new List<(DateTime Start, DateTime End)>();

            var singleRounds = await _context.CandidateInterviewRounds
                .Where(r => r.ScheduledDate.HasValue
                            && r.ScheduledDate.Value.Date == date
                            && r.InterviewerID == interviewerId)
                .ToListAsync();

            foreach (var r in singleRounds)
            {
                var interval = RoundToInterval(r);
                if (interval.HasValue) result.Add(interval.Value);
            }

            var panelMemberRounds = await _context.CandidatePanelMembers
                .Where(pm => pm.InterviewerID == interviewerId)
                .Select(pm => pm.CandidateRound)
                .Where(r => r.ScheduledDate.HasValue && r.ScheduledDate.Value.Date == date)
                .ToListAsync();

            foreach (var r in panelMemberRounds)
            {
                var interval = RoundToInterval(r);
                if (interval.HasValue) result.Add(interval.Value);
            }


            return result;
        }

        public async Task<int> CountInterviewerInterviewsOnDateAsync(string interviewerId, DateTime date)
        {
            date = date.Date;
            var singleCount = await _context.CandidateInterviewRounds
                .CountAsync(r => r.ScheduledDate.HasValue
                                 && r.ScheduledDate.Value.Date == date
                                 && r.InterviewerID == interviewerId);

            var panelCount = await _context.CandidatePanelMembers
                .Where(pm => pm.InterviewerID == interviewerId
                             && pm.CandidateRound.ScheduledDate.HasValue
                             && pm.CandidateRound.ScheduledDate.Value.Date == date)
                .CountAsync();

            return singleCount + panelCount;
        }

        public async Task<int> CountCandidateInterviewsOnDateAsync(int candidateId, DateTime date)
        {
            date = date.Date;
            var c = await _context.CandidateInterviewRounds
                .CountAsync(r => r.CandidateID == candidateId
                                 && r.ScheduledDate.HasValue
                                 && r.ScheduledDate.Value.Date == date);
            return c;
        }

        public async Task<List<(DateTime Start, DateTime End)>> GetBusyIntervalsForCandidateAsync(int candidateId, DateTime date)
        {
            date = date.Date;
            var result = new List<(DateTime Start, DateTime End)>();
            var rounds = await _context.CandidateInterviewRounds
                .Where(r => r.CandidateID == candidateId
                            && r.ScheduledDate.HasValue
                            && r.ScheduledDate.Value.Date == date)
                .ToListAsync();

            foreach (var r in rounds)
            {
                if (!r.ScheduledDate.HasValue || !r.StartTime.HasValue || !r.DurationMinutes.HasValue) continue;
                var start = r.ScheduledDate.Value.Date + r.StartTime.Value;
                var end = start.AddMinutes(r.DurationMinutes.Value);
                result.Add((start, end));
            }

            return result;
        }
    }
}