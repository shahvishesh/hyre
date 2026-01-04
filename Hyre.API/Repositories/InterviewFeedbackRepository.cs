using Hyre.API.Data;
using Hyre.API.Interfaces.CandidateFeedback;
using Hyre.API.Models;
using Microsoft.EntityFrameworkCore;

namespace Hyre.API.Repositories
{
    public class InterviewFeedbackRepository : IInterviewFeedbackRepository
    {
        private readonly ApplicationDbContext _context;

        public InterviewFeedbackRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<bool> HasAccessToRoundAsync(int roundId, string interviewerId)
        {
            return await _context.CandidateInterviewRounds
                .AnyAsync(r =>
                    r.CandidateRoundID == roundId &&
                    (r.InterviewerID == interviewerId ||
                     r.PanelMembers.Any(p => p.InterviewerID == interviewerId)));
        }

        public async Task<bool> FeedbackExistsAsync(int roundId, string interviewerId)
        {
            return await _context.CandidateInterviewFeedbacks
                .AnyAsync(f => f.CandidateRoundID == roundId &&
                               f.InterviewerID == interviewerId);
        }

        public async Task AddFeedbackAsync(CandidateInterviewFeedback feedback)
        {
            await _context.CandidateInterviewFeedbacks.AddAsync(feedback);
        }

        public async Task<List<CandidateInterviewFeedback>> GetFeedbacksByRoundAsync(int roundId)
        {
            return await _context.CandidateInterviewFeedbacks
                .Include(f => f.Interviewer)
                .Include(f => f.SkillRatings)
                    .ThenInclude(sr => sr.Skill)
                .Where(f => f.CandidateRoundID == roundId)
                .ToListAsync();
        }

        public async Task<List<CandidateInterviewFeedback>> GetFeedbacksByInterviewerAsync(string interviewerId)
        {
            return await _context.CandidateInterviewFeedbacks
                .Include(f => f.Interviewer)
                .Include(f => f.SkillRatings)
                    .ThenInclude(sr => sr.Skill)
                .Where(f => f.InterviewerID == interviewerId)
                .ToListAsync();
        }

        public async Task<List<CandidateInterviewRound>> GetCompletedRoundsForInterviewerAsync(string interviewerId)
        {
            return await _context.CandidateInterviewRounds
                .Include(r => r.Candidate)
                .Include(r => r.Job)
                .Include(r => r.PanelMembers)
                .Where(r =>
                    r.Status == "Completed" &&
                    (r.InterviewerID == interviewerId ||
                     r.PanelMembers.Any(pm => pm.InterviewerID == interviewerId)))
                .ToListAsync();
        }

        public async Task<List<Job>> GetJobsForInterviewerAsync(string interviewerId)
        {
            return await _context.CandidateInterviewRounds
                .Include(r => r.Job)
                .Where(r => r.InterviewerID == interviewerId ||
                           r.PanelMembers.Any(pm => pm.InterviewerID == interviewerId))
                .Select(r => r.Job)
                .Distinct()
                .OrderByDescending(j => j.CreatedAt)
                .ToListAsync();
        }

        public async Task<List<Candidate>> GetInterviewedCandidatesForJobAsync(int jobId, string interviewerId)
        {
            return await _context.CandidateInterviewRounds
                .Include(r => r.Candidate)
                    .ThenInclude(c => c.CandidateSkills)
                        .ThenInclude(cs => cs.Skill)
                .Where(r => r.JobID == jobId &&
                           (r.InterviewerID == interviewerId ||
                            r.PanelMembers.Any(pm => pm.InterviewerID == interviewerId)))
                .Select(r => r.Candidate)
                .Distinct()
                .OrderBy(c => c.FirstName)
                .ThenBy(c => c.LastName)
                .ToListAsync();
        }

        public async Task<List<CandidateInterviewRound>> GetCompletedRoundsForCandidateJobAsync(int candidateId, int jobId, string interviewerId)
        {
            return await _context.CandidateInterviewRounds
                .Include(r => r.Candidate)
                .Include(r => r.Job)
                .Include(r => r.PanelMembers)
                .Where(r =>
                    r.CandidateID == candidateId &&
                    r.JobID == jobId &&
                    r.Status == "Completed" &&
                    (r.InterviewerID == interviewerId ||
                     r.PanelMembers.Any(pm => pm.InterviewerID == interviewerId)))
                .ToListAsync();
        }

        public async Task SaveAsync()
        {
            await _context.SaveChangesAsync();
        }
    }
}
