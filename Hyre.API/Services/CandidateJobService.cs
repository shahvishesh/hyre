using Hyre.API.Data;
using Hyre.API.Dtos.CandidateMatching;
using Hyre.API.Interfaces.CandidateMatching;
using Hyre.API.Models;
using Microsoft.EntityFrameworkCore;

namespace Hyre.API.Services
{
    public class CandidateJobService : ICandidateJobService
    {
        private readonly ApplicationDbContext _context;

        public CandidateJobService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<CandidateJobResponseDto> LinkCandidateAsync(int jobId, CreateCandidateLinkDto dto, string createdByUserId)
        {
            var job = await _context.Jobs.FirstOrDefaultAsync(j => j.JobID == jobId);
            if (job == null)
                throw new Exception("Job not found.");

            var candidate = await _context.Candidates.FirstOrDefaultAsync(c => c.CandidateID == dto.CandidateID);
            if (candidate == null)
                throw new Exception("Candidate not found.");

            var existing = await _context.CandidateJobs
                .FirstOrDefaultAsync(cj => cj.JobID == jobId && cj.CandidateID == dto.CandidateID);
            if (existing != null)
                throw new Exception("Candidate is already linked to this job.");

            var candidateJob = new CandidateJob
            {
                CandidateID = dto.CandidateID,
                JobID = jobId,
                Stage = "Screening",
                CreatedBy = createdByUserId,
                CreatedAt = DateTime.Now,
                UpdatedAt = DateTime.Now
            };

            await _context.CandidateJobs.AddAsync(candidateJob);
            await _context.SaveChangesAsync();

            return new CandidateJobResponseDto(
                candidateJob.CandidateJobID,
                candidateJob.CandidateID,
                candidateJob.JobID,
                candidateJob.Stage,
                candidateJob.CreatedAt
            );
        }

        public async Task<LinkedCandidatesResponseDto> GetLinkedCandidatesAsync(int jobId)
        {
            var job = await _context.Jobs.FirstOrDefaultAsync(j => j.JobID == jobId);
            if (job == null)
                throw new Exception("Job not found.");

            var reviewedCandidateJobIds = await _context.CandidateReviews
                .Where(r => r.CandidateJob.JobID == jobId)
                .Select(r => r.CandidateJobID)
                .ToListAsync();

            var linkedCandidates = await _context.CandidateJobs
                .Include(cj => cj.Candidate)
                .ThenInclude(c => c.CandidateSkills)
                .ThenInclude(cs => cs.Skill)
                .Where(cj => cj.JobID == jobId && !reviewedCandidateJobIds.Contains(cj.CandidateJobID)) 
                .OrderBy(cj => cj.CreatedAt)
                .Select(cj => new LinkedCandidateDto(
                    cj.CandidateJobID,
                    cj.CandidateID,
                    $"{cj.Candidate.FirstName} {cj.Candidate.LastName}".Trim(),
                    cj.Candidate.Email,
                    cj.Candidate.Phone,
                    cj.Candidate.ExperienceYears,
                    cj.Stage,
                    cj.CreatedAt,
                    cj.UpdatedAt,
                    cj.Candidate.CandidateSkills.Select(cs => new CandidateSkillSummaryDto(
                        cs.SkillID,
                        cs.Skill.SkillName,
                        cs.YearsOfExperience
                    )).ToList()
                ))
                .ToListAsync();

            return new LinkedCandidatesResponseDto(
                jobId,
                job.Title,
                linkedCandidates,
                linkedCandidates.Count
            );
        }

    }
}
