using Hyre.API.Data;
using Hyre.API.Dtos.CandidateMatching;
using Hyre.API.Interfaces.CandidateMatching;
using Hyre.API.Models;
using Microsoft.EntityFrameworkCore;

namespace Hyre.API.Services
{
    public class CandidateMatchingService : ICandidateMatchingService
    {
        private readonly ApplicationDbContext _context;

        public CandidateMatchingService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<MatchResultDto> GetMatchingCandidatesAsync(int jobId)
        {
            var job = await _context.Jobs
                .Include(j => j.JobSkills).ThenInclude(js => js.Skill)
                .FirstOrDefaultAsync(j => j.JobID == jobId);

            if (job == null)
                throw new Exception("Job not found.");

            var required = job.JobSkills
                .Where(js => js.SkillType == "Required")
                .Select(js => js.Skill.SkillName)
                .ToList();

            var preferred = job.JobSkills
                .Where(js => js.SkillType == "Preferred")
                .Select(js => js.Skill.SkillName)
                .ToList();

            var candidates = await _context.Candidates
                .Include(c => c.CandidateSkills).ThenInclude(cs => cs.Skill)
                .ToListAsync();

            var matches = new List<CandidateMatchDto>();

            foreach (var candidate in candidates)
            {
                double score = ComputeMatchScore(job, candidate);

                var candidateSkills = candidate.CandidateSkills.Select(cs => cs.Skill.SkillName).ToList();

                var matchedRequired = required.Intersect(candidateSkills, StringComparer.OrdinalIgnoreCase).ToList();
                var missingRequired = required.Except(candidateSkills, StringComparer.OrdinalIgnoreCase).ToList();

                var matchedPreferred = preferred.Intersect(candidateSkills, StringComparer.OrdinalIgnoreCase).ToList();
                var missingPreferred = preferred.Except(candidateSkills, StringComparer.OrdinalIgnoreCase).ToList();

                matches.Add(new CandidateMatchDto(
                    candidate.CandidateID,
                    $"{candidate.FirstName} {candidate.LastName}",
                    candidate.Email,
                    candidate.ExperienceYears,
                    Math.Round(score, 2),
                    matchedRequired,
                    missingRequired,
                    matchedPreferred,
                    missingPreferred
                ));
            }

            return new MatchResultDto(job.JobID, job.Title, matches.OrderByDescending(m => m.MatchScore).ToList());
        }


        private double ComputeMatchScore(Job job, Candidate candidate)
        {
            if (job == null || candidate == null || !candidate.ExperienceYears.HasValue)
                return 0;

            var required = job.JobSkills.Where(js => js.SkillType == "Required")
                .Select(js => js.Skill.SkillName).ToList();

            var preferred = job.JobSkills.Where(js => js.SkillType == "Preferred")
                .Select(js => js.Skill.SkillName).ToList();

            var candidateSkills = candidate.CandidateSkills.ToList();

            double skillScore = CalculateSkillScore(candidateSkills, required, preferred);
            decimal avgSkillExp = CalculateAverageSkillExperience(candidateSkills, required);
            decimal totalExpScore = GetExperienceScore(candidate.ExperienceYears.Value, job.MinExperience, job.MaxExperience);
            decimal perSkillExpScore = GetExperienceScore(avgSkillExp, job.MinExperience, job.MaxExperience);

            double combinedExpScore = (double)(totalExpScore * 0.4m + perSkillExpScore * 0.6m);
            double finalScore = (skillScore * 0.7) + (combinedExpScore * 0.3);

            return Math.Round(finalScore, 2);
        }


        private double CalculateSkillScore(List<CandidateSkill> candidateSkills, List<string> requiredSkills, List<string> preferredSkills)
        {
            double requiredScore = 0, preferredScore = 0;

            if (requiredSkills.Any())
            {
                int matched = candidateSkills.Count(s =>
                    requiredSkills.Contains(s.Skill.SkillName, StringComparer.OrdinalIgnoreCase));
                requiredScore = (matched / (double)requiredSkills.Count) * 70;
            }

            if (preferredSkills.Any())
            {
                int matched = candidateSkills.Count(s =>
                    preferredSkills.Contains(s.Skill.SkillName, StringComparer.OrdinalIgnoreCase));
                preferredScore = (matched / (double)preferredSkills.Count) * 30;
            }

            return requiredScore + preferredScore; // 0–100 scale
        }

        private decimal GetExperienceScore(decimal candidateExp, decimal? minExp, decimal? maxExp)
        {
            if (minExp == null || maxExp == null)
                return 100;

            if (candidateExp < minExp)
            {
                var ratio = candidateExp / minExp.Value;
                return Math.Clamp(ratio * 100, 0, 100);
            }

            if (candidateExp > maxExp)
            {
                var over = candidateExp - maxExp.Value;
                var penalty = Math.Min(over / maxExp.Value * 10, 10);
                return 100 - penalty;
            }

            return 100;
        }

        private decimal CalculateAverageSkillExperience(List<CandidateSkill> candidateSkills, List<string> requiredSkills)
        {
            var matchedSkills = candidateSkills
                .Where(s => requiredSkills.Contains(s.Skill.SkillName, StringComparer.OrdinalIgnoreCase))
                .ToList();

            if (!matchedSkills.Any())
                return 0;

            return matchedSkills.Average(s => s.YearsOfExperience ?? 0);
        }


    }
}
