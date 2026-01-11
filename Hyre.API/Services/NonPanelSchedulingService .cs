using DocumentFormat.OpenXml.Bibliography;
using Hyre.API.Dtos.Scheduling;
using Hyre.API.Interfaces.Candidates;
using Hyre.API.Interfaces.Scheduling;
using Hyre.API.Models;
using Microsoft.AspNetCore.Identity;

namespace Hyre.API.Services
{
    public class NonPanelSchedulingService : INonPanelSchedulingService
    {
        private readonly IInterviewScheduleRepository _repo;

        //private readonly TimeSpan DayStart = TimeSpan.FromHours(10);
        //private readonly TimeSpan DayEnd = TimeSpan.FromHours(16);
        private readonly TimeSpan DayStart = new TimeSpan(4, 30, 0); // 04:30 UTC = 10:00 IST
        private readonly TimeSpan DayEnd = new TimeSpan(10, 30, 0); // 10:30 UTC = 16:00 IST

        private readonly TimeSpan BreakGap = TimeSpan.FromMinutes(30);
        private readonly TimeSpan Granularity = TimeSpan.FromMinutes(15);
        private const int MaxInterviewsPerDay = 3;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ICandidateService _candidateService;


        public NonPanelSchedulingService(IInterviewScheduleRepository repo, UserManager<ApplicationUser> userManager, ICandidateService candidateService)
        {
            _repo = repo;
            _userManager = userManager;
            _candidateService = candidateService;
        }

        public async Task<List<AvailableSlotDto>> GetAvailableSlotsAsync(NonPanelAvailabilityRequestDto request)
        {
            //var date = request.Date.Date;
            var date = DateTime.SpecifyKind(request.Date.Date, DateTimeKind.Utc);

            ValidateDateRules(date);

            var user = await _userManager.FindByIdAsync(request.InterviewerId);
            if (user == null)
            {
                throw new Exception($"Interviewer with ID {request.InterviewerId} not found");
            }

            var candidateExists = await _candidateService.CandidateExistsAsync(request.CandidateId);
            if (!candidateExists)
                throw new Exception($"Candidate with ID {request.CandidateId} not found");

            var interviewerCount = await _repo.CountInterviewerInterviewsOnDateAsync(request.InterviewerId, date);
            if (interviewerCount >= MaxInterviewsPerDay)
                return new List<AvailableSlotDto>();

            // RULE: Max 3 interviews/day for candidate
            var candidateCount = await _repo.CountCandidateInterviewsOnDateAsync(request.CandidateId, date);
            if (candidateCount >= MaxInterviewsPerDay)
                return new List<AvailableSlotDto>();

            var interviewerBusy = await _repo.GetBusyIntervalsForInterviewerAsync(request.InterviewerId, date);
            var candidateBusy = await _repo.GetBusyIntervalsForCandidateAsync(request.CandidateId, date);

            interviewerBusy = interviewerBusy.OrderBy(x => x.Start).ToList();
            candidateBusy = candidateBusy.OrderBy(x => x.Start).ToList();

            var interviewerFree = ComputeFreeWindows(interviewerBusy, date);
            var candidateFree = ComputeFreeWindows(candidateBusy, date);

            var intersected = IntersectWindows(interviewerFree, candidateFree);

            
            var slots = GenerateSlotsFromFreeWindows(intersected, request.DurationMinutes, date);

            return slots.Select(s => new AvailableSlotDto(s.Start, s.End)).ToList();
        }


        private void ValidateDateRules(DateTime date)
        {
            var now = DateTime.UtcNow;

            // 24 hours in advance
            if (date < now.AddHours(24).Date)
                throw new ArgumentException("Interviews must be scheduled at least 24 hours in advance.");

            // No weekends
            if (date.DayOfWeek is DayOfWeek.Saturday or DayOfWeek.Sunday)
                throw new ArgumentException("Cannot schedule interviews on weekends.");

            // Not more than 30 days ahead
            if (date > now.Date.AddDays(30))
                throw new ArgumentException("Cannot schedule more than 30 days in advance.");
        }

 
        private List<(DateTime Start, DateTime End)> ComputeFreeWindows(
            List<(DateTime Start, DateTime End)> busy,
            DateTime date)
        {
            var free = new List<(DateTime, DateTime)>();
            //var startOfDay = date.Date + DayStart;
            //var endOfDay = date.Date + DayEnd;
            var startOfDay = DateTime.SpecifyKind(date.Date + DayStart, DateTimeKind.Utc);
            var endOfDay = DateTime.SpecifyKind(date.Date + DayEnd, DateTimeKind.Utc);


            if (busy.Count == 0)
            {
                free.Add((startOfDay, endOfDay));
                return free;
            }

            var firstFreeEnd = busy[0].Start - BreakGap;
            if (firstFreeEnd > startOfDay)
                free.Add((startOfDay, firstFreeEnd));

            for (int i = 0; i < busy.Count - 1; i++)
            {
                var endPrev = busy[i].End + BreakGap;
                var startNext = busy[i + 1].Start - BreakGap;

                if (startNext > endPrev)
                    free.Add((endPrev, startNext));
            }

            var lastFreeStart = busy.Last().End + BreakGap;
            if (lastFreeStart < endOfDay)
                free.Add((lastFreeStart, endOfDay));

            return free;
        }

        
        private List<(DateTime Start, DateTime End)> IntersectWindows(
            List<(DateTime Start, DateTime End)> a,
            List<(DateTime Start, DateTime End)> b)
        {
            var res = new List<(DateTime, DateTime)>();
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

       
        private List<(DateTime Start, DateTime End)> GenerateSlotsFromFreeWindows(
            List<(DateTime Start, DateTime End)> freeWindows,
            int durationMinutes,
            DateTime date)
        {
            var now = DateTime.UtcNow;
            var duration = TimeSpan.FromMinutes(durationMinutes);
            var slots = new List<(DateTime, DateTime)>();

            foreach (var win in freeWindows)
            {
                var start = RoundUpToGranularity(win.Start, Granularity);
                var lastPossibleStart = win.End - duration;

                for (var s = start; s <= lastPossibleStart; s = s.Add(Granularity))
                {
                    var e = s.Add(duration);

                    if (s < now.AddHours(24)) continue;

                    if (s.DayOfWeek is DayOfWeek.Saturday or DayOfWeek.Sunday) continue;

                    slots.Add((s, e));
                }
            }

            return slots;
        }

        private DateTime RoundUpToGranularity(DateTime dt, TimeSpan granularity)
        {
            long ticks = (long)Math.Ceiling(dt.Ticks / (double)granularity.Ticks) * granularity.Ticks;
            return new DateTime(ticks, DateTimeKind.Utc);
        }
    }
}
