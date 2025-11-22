using Hyre.API.Dtos.Scheduling;
using Hyre.API.Interfaces.Scheduling;

namespace Hyre.API.Services
{
    public class PanelSchedulingService : IPanelSchedulingService
    {
        private readonly IInterviewScheduleRepository _repo;
        private readonly TimeSpan DayStart = TimeSpan.FromHours(10); // 10:00
        private readonly TimeSpan DayEnd = TimeSpan.FromHours(16);   // 16:00
        private readonly TimeSpan BreakGap = TimeSpan.FromMinutes(30); // required break
        private readonly TimeSpan Granularity = TimeSpan.FromMinutes(15); // slot step
        private readonly int MaxInterviewsPerDay = 3;

        public PanelSchedulingService(IInterviewScheduleRepository repo)
        {
            _repo = repo;
        }

        public async Task<List<AvailableSlotDto>> GetAvailablePanelSlotsAsync(PanelAvailabilityRequestDto request)
        {
            var date = request.Date.Date;
            ValidateDateConstraints(date);

            if (await _repo.CountCandidateInterviewsOnDateAsync(request.CandidateId, date) >= MaxInterviewsPerDay)
                return new List<AvailableSlotDto>();

            foreach (var interviewerId in request.InterviewerIds.Distinct())
            {
                if (await _repo.CountInterviewerInterviewsOnDateAsync(interviewerId, date) >= MaxInterviewsPerDay)
                    return new List<AvailableSlotDto>();
            }

            var allBusyByInterviewer = new List<List<(DateTime Start, DateTime End)>>();
            foreach (var interviewerId in request.InterviewerIds.Distinct())
            {
                var busy = await _repo.GetBusyIntervalsForInterviewerAsync(interviewerId, date);
                
                var normalized = busy
                    .Select(b => (Start: b.Start, End: b.End))
                    .OrderBy(b => b.Start)
                    .ToList();
                allBusyByInterviewer.Add(normalized);
            }

            var freeByInterviewer = new List<List<(DateTime Start, DateTime End)>>();
            foreach (var busy in allBusyByInterviewer)
            {
                var free = ComputeFreeWindowsFromBusy(busy, date);
                freeByInterviewer.Add(free);
            }

            var intersected = freeByInterviewer.FirstOrDefault() ?? new List<(DateTime, DateTime)>();
            for (int i = 1; i < freeByInterviewer.Count; i++)
            {
                intersected = IntersectWindows(intersected, freeByInterviewer[i]);
                if (!intersected.Any()) break; // no common window
            }

            var mergedIntersected = MergeDateTimeIntervals(intersected);

            var candidateSlots = GenerateSlotsFromFreeWindows(mergedIntersected, request.DurationMinutes, date, request.CandidateId);

            return candidateSlots.Select(s => new AvailableSlotDto(s.Start, s.End)).ToList();
        }

        private void ValidateDateConstraints(DateTime date)
        {
            var now = DateTime.UtcNow;
            if (date.Date < now.AddHours(24).Date && date.Date == now.Date)
                throw new ArgumentException("Selected date must be at least 24 hours from now.");

            if (date.Date < now.Date.AddDays(1) && (now.AddHours(24) > date)) // robust check
                throw new ArgumentException("Selected date must be at least 24 hours from now.");

            if (date.DayOfWeek == DayOfWeek.Saturday || date.DayOfWeek == DayOfWeek.Sunday)
                throw new ArgumentException("Cannot schedule on weekends.");

            if (date > now.Date.AddDays(30))
                throw new ArgumentException("Cannot schedule more than 30 days in advance.");
        }

        private List<(DateTime Start, DateTime End)> ComputeFreeWindowsFromBusy(List<(DateTime Start, DateTime End)> busySorted, DateTime date)
        {
            var free = new List<(DateTime Start, DateTime End)>();
            var dayStart = date.Date + DayStart;
            var dayEnd = date.Date + DayEnd;

            if (busySorted == null || busySorted.Count == 0)
            {
                free.Add((dayStart, dayEnd));
                return free;
            }

            var mergedBusy = MergeDateTimeIntervals(busySorted);

            var firstStart = mergedBusy[0].Start;
            var firstFreeEnd = firstStart - BreakGap;
            if (firstFreeEnd > dayStart)
                free.Add((dayStart, firstFreeEnd));

            for (int i = 0; i < mergedBusy.Count - 1; i++)
            {
                var prevEnd = mergedBusy[i].End;
                var nextStart = mergedBusy[i + 1].Start;
                var freeStart = prevEnd + BreakGap;
                var freeEnd = nextStart - BreakGap;
                if (freeEnd > freeStart)
                    free.Add((freeStart, freeEnd));
            }

            var lastEnd = mergedBusy.Last().End + BreakGap;
            if (lastEnd < dayEnd)
                free.Add((lastEnd, dayEnd));

            return free;
        }

        private List<(DateTime Start, DateTime End)> MergeDateTimeIntervals(List<(DateTime Start, DateTime End)> intervals)
        {
            var result = new List<(DateTime Start, DateTime End)>();
            if (intervals == null || intervals.Count == 0) return result;

            var sorted = intervals.OrderBy(i => i.Start).ToList();
            var current = sorted[0];

            for (int i = 1; i < sorted.Count; i++)
            {
                var next = sorted[i];
                if (next.Start <= current.End)
                {
                    current = (current.Start, next.End > current.End ? next.End : current.End);
                }
                else
                {
                    result.Add(current);
                    current = next;
                }
            }
            result.Add(current);
            return result;
        }

        private List<(DateTime Start, DateTime End)> IntersectWindows(List<(DateTime Start, DateTime End)> a, List<(DateTime Start, DateTime End)> b)
        {
            var res = new List<(DateTime Start, DateTime End)>();
            int i = 0, j = 0;
            while (i < a.Count && j < b.Count)
            {
                var start = a[i].Start > b[j].Start ? a[i].Start : b[j].Start;
                var end = a[i].End < b[j].End ? a[i].End : b[j].End;

                if (start < end)
                    res.Add((start, end));

                if (a[i].End < b[j].End) i++;
                else j++;
            }
            return res;
        }

        private List<(DateTime Start, DateTime End)> GenerateSlotsFromFreeWindows(List<(DateTime Start, DateTime End)> freeWindows, int durationMinutes, DateTime date, int candidateId)
        {
            var slots = new List<(DateTime Start, DateTime End)>();
            var duration = TimeSpan.FromMinutes(durationMinutes);
            var now = DateTime.UtcNow;

            foreach (var win in freeWindows)
            {
                var start = RoundUpToGranularity(win.Start, Granularity);
                var lastPossibleStart = win.End - duration;

                for (var s = start; s <= lastPossibleStart; s = s.Add(Granularity))
                {
                    var e = s.Add(duration);

                    if (s < now.AddHours(24)) continue;


                    if (s.DayOfWeek == DayOfWeek.Saturday || s.DayOfWeek == DayOfWeek.Sunday) continue;

                    if (s.Date > now.Date.AddDays(30)) continue;

                    slots.Add((s, e));
                }
            }

            return slots;
        }

        private DateTime RoundUpToGranularity(DateTime dt, TimeSpan gran)
        {
            var ticks = (long)Math.Ceiling(dt.Ticks / (double)gran.Ticks) * gran.Ticks;
            return new DateTime(ticks, DateTimeKind.Utc);
        }
    }
}
