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

    }
}
