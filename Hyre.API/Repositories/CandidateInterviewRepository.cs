using Hyre.API.Data;
using Hyre.API.Interfaces.Scheduling;
using Hyre.API.Models;
using Microsoft.EntityFrameworkCore;

namespace Hyre.API.Repositories
{
    public class CandidateInterviewRepository : ICandidateInterviewRepository
    {
        private readonly ApplicationDbContext _context;

        public CandidateInterviewRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task AddCandidateInterviewRoundAsync(CandidateInterviewRound round)
        {
            await _context.CandidateInterviewRounds.AddAsync(round);
        }

        public async Task AddCandidatePanelMembersAsync(IEnumerable<CandidatePanelMember> members)
        {
            await _context.CandidatePanelMembers.AddRangeAsync(members);
        }

        public async Task<bool> IsInterviewerAvailableAsync(string interviewerId, DateTime startUtc, DateTime endUtc)
        {
            // Check single-interviewer rounds where interviewer is directly assigned
            var conflictSingle = await _context.CandidateInterviewRounds
                .AnyAsync(r => r.InterviewerID == interviewerId
                               && r.ScheduledDate.HasValue
                               && (r.ScheduledDate.Value.Date + r.StartTime.Value) < endUtc
                               && (r.ScheduledDate.Value.Date + r.StartTime.Value).AddMinutes(r.DurationMinutes ?? 0) > startUtc);

            if (conflictSingle) return false;

            // Check panel rounds membership
            var conflictPanel = await _context.CandidatePanelMembers
                .Include(pm => pm.CandidateRound)
                .AnyAsync(pm => pm.InterviewerID == interviewerId
                                && pm.CandidateRound.ScheduledDate.HasValue
                                && (pm.CandidateRound.ScheduledDate.Value.Date + pm.CandidateRound.StartTime.Value) < endUtc
                                && (pm.CandidateRound.ScheduledDate.Value.Date + pm.CandidateRound.StartTime.Value).AddMinutes(pm.CandidateRound.DurationMinutes ?? 0) > startUtc);

            return !conflictPanel;
        }

        public async Task<bool> IsCandidateAvailableAsync(int candidateId, DateTime startUtc, DateTime endUtc)
        {
            return !await _context.CandidateInterviewRounds
                .AnyAsync(r => r.CandidateID == candidateId
                               && r.ScheduledDate.HasValue
                               && (r.ScheduledDate.Value.Date + r.StartTime.Value) < endUtc
                               && (r.ScheduledDate.Value.Date + r.StartTime.Value).AddMinutes(r.DurationMinutes ?? 0) > startUtc);
        }

        public async Task<int> CountInterviewerInterviewsOnDateAsync(string interviewerId, DateTime date)
        {
            date = date.Date;
            var single = await _context.CandidateInterviewRounds
                .CountAsync(r => r.ScheduledDate.HasValue
                                 && r.ScheduledDate.Value.Date == date
                                 && r.InterviewerID == interviewerId);

            var panel = await _context.CandidatePanelMembers
                .Include(pm => pm.CandidateRound)
                .CountAsync(pm => pm.InterviewerID == interviewerId
                                  && pm.CandidateRound.ScheduledDate.HasValue
                                  && pm.CandidateRound.ScheduledDate.Value.Date == date);

            return single + panel;
        }

        public async Task<int> CountCandidateInterviewsOnDateAsync(int candidateId, DateTime date)
        {
            date = date.Date;
            return await _context.CandidateInterviewRounds
                .CountAsync(r => r.CandidateID == candidateId
                                 && r.ScheduledDate.HasValue
                                 && r.ScheduledDate.Value.Date == date);
        }

        public async Task<CandidateInterviewRound?> GetRoundByIdAsync(int id)
        {
            return await _context.CandidateInterviewRounds
                .Include(r => r.PanelMembers)
                .FirstOrDefaultAsync(r => r.CandidateRoundID == id);
        }

        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }
    }
}
