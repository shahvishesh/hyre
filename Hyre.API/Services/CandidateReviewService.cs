using Hyre.API.Data;
using Hyre.API.Interfaces.CandidateReview;
using Hyre.API.Models;
using Microsoft.EntityFrameworkCore;
using static Hyre.API.Dtos.CandidateReview.ReviewDtos;

namespace Hyre.API.Services
{
    public class CandidateReviewService : ICandidateReviewService
    {
        private readonly ApplicationDbContext _context;

        public CandidateReviewService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<ReviewResponseDto> CreateReviewAsync(CreateReviewDto dto, string reviewerId)
        {
            
            var candidateJob = await _context.CandidateJobs.FindAsync(dto.CandidateJobID);
            if (candidateJob == null) throw new Exception("CandidateJob not found.");

            
            var existing = await _context.CandidateReviews
                .Include(r => r.SkillReviews)
                    .ThenInclude(sr => sr.Skill)
                .FirstOrDefaultAsync(r => r.CandidateJobID == dto.CandidateJobID);

            if (existing != null)
            {
                //return new ReviewResponseDto(existing.ReviewID, existing.CandidateJobID, existing.Reviewer.FirstName, existing.Decision, existing.Comment, existing.RecruiterDecision, existing.ReviewedAt);
                return await GetReviewResponseDtoAsync(existing.ReviewID);
            }

            var review = new CandidateReview
            {
                CandidateJobID = dto.CandidateJobID,
                ReviewerId = reviewerId,
                Comment = dto.Comment,
                Decision = dto.Decision ?? "Pending",
                ReviewedAt = DateTime.UtcNow
            };

            if (dto.Skills != null && dto.Skills.Any())
            {
                foreach (var s in dto.Skills)
                {
                    review.SkillReviews.Add(new CandidateSkillReview
                    {
                        SkillId = s.SkillId,
                        IsVerified = s.IsVerified,
                        VerifiedYearsOfExperience = s.VerifiedYearsOfExperience
                    });
                }
            }

            _context.CandidateReviews.Add(review);
            await _context.SaveChangesAsync();

            return await GetReviewResponseDtoAsync(review.ReviewID);
        }

        public async Task<ReviewResponseDto> UpdateReviewAsync(UpdateReviewDto dto, string reviewerId)
        {
            var review = await _context.CandidateReviews
                .Include(r => r.SkillReviews)
                .ThenInclude(sr => sr.Skill)
                .Include(r => r.Reviewer)
                .FirstOrDefaultAsync(r => r.ReviewID == dto.ReviewID);

            if (review == null)
                throw new Exception("Review not found.");

            if (review.ReviewerId != reviewerId)
                throw new UnauthorizedAccessException("Only the original reviewer can modify this review.");

            review.Comment = dto.Comment;
            review.Decision = dto.Decision ?? review.Decision;
            review.ReviewedAt = DateTime.UtcNow;

            if (dto.Skills != null && dto.Skills.Any())
            {
                var existingSkills = review.SkillReviews.ToDictionary(sr => sr.SkillId, sr => sr);
                var updatedSkills = dto.Skills.ToDictionary(s => s.SkillId, s => s);

                //Update existing ones
                foreach (var existing in existingSkills.Values)
                {
                    if (updatedSkills.TryGetValue(existing.SkillId, out var updated))
                    {
                        existing.IsVerified = updated.IsVerified;
                        existing.VerifiedYearsOfExperience = updated.VerifiedYearsOfExperience;
                    }
                    else
                    {
                        _context.CandidateSkillReviews.Remove(existing);
                    }
                }

                //Add new ones
                var newSkillIds = updatedSkills.Keys.Except(existingSkills.Keys);
                foreach (var newSkillId in newSkillIds)
                {
                    var s = updatedSkills[newSkillId];
                    review.SkillReviews.Add(new CandidateSkillReview
                    {
                        CandidateReviewId = review.ReviewID,
                        SkillId = s.SkillId,
                        IsVerified = s.IsVerified,
                        VerifiedYearsOfExperience = s.VerifiedYearsOfExperience
                    });
                }
            }

            await _context.SaveChangesAsync();
            return await GetReviewResponseDtoAsync(review.ReviewID);
        }


        private async Task<ReviewResponseDto> GetReviewResponseDtoAsync(int reviewId)
        {
            var r = await _context.CandidateReviews
                .Include(x => x.Reviewer)
                .Include(x => x.SkillReviews).ThenInclude(sr => sr.Skill)
                .FirstOrDefaultAsync(x => x.ReviewID == reviewId);

            var skills = r.SkillReviews.Select(sr => new ReviewedSkillDto(
                sr.SkillId,
                sr.IsVerified,
                sr.VerifiedYearsOfExperience
            )).ToList();

            return new ReviewResponseDto(
                r.ReviewID,
                r.CandidateJobID,
                r.Reviewer?.FirstName ?? "Unknown",
                r.Decision,
                r.Comment,
                r.RecruiterDecision,
                r.ReviewedAt,
                skills
            );
        }


        public async Task AddCommentAsync(AddCommentDto dto, string commenterId)
        {
            var review = await _context.CandidateReviews.FindAsync(dto.ReviewID)
                ?? throw new Exception("Review not found.");

            var comment = new CandidateReviewComment
            {
                CandidateReviewID = dto.ReviewID,
                CommenterId = commenterId,
                CommentText = dto.CommentText
            };

            _context.CandidateReviewComments.Add(comment);
            await _context.SaveChangesAsync();
        }

        public async Task ApplyRecruiterDecisionAsync(RecruiterDecisionDto dto, string recruiterId)
        {
            var review = await _context.CandidateReviews.FindAsync(dto.ReviewID)
                ?? throw new Exception("Review not found.");

            review.RecruiterId = recruiterId;
            review.RecruiterDecision = dto.Decision;
            review.RecruiterActionAt = DateTime.UtcNow;

            // Recruiter may also move the candidate to the next stage
            var candidateJob = await _context.CandidateJobs.FindAsync(review.CandidateJobID);
            if (candidateJob != null)
            {
                candidateJob.Stage = dto.Decision == "Shortlisted" ? "Interview" : "Rejected";
            }

            await _context.SaveChangesAsync();
        }

        public async Task<IEnumerable<ReviewResponseDto>> GetReviewsByJobAsync(int jobId)
        {
            var reviews = await _context.CandidateReviews
                .Include(r => r.Reviewer)
                .Include(r => r.Recruiter)
                .Include(r => r.CandidateJob)
                .Include(r => r.SkillReviews).ThenInclude(sr => sr.Skill)
                .Where(r => r.CandidateJob.JobID == jobId)
                .ToListAsync();


            var response = new List<ReviewResponseDto>();

            foreach (var review in reviews)
            {
                var skills = review.SkillReviews.Select(sr => new ReviewedSkillDto(
                    sr.SkillId,
                    sr.IsVerified,
                    sr.VerifiedYearsOfExperience
                )).ToList();
                response.Add(new ReviewResponseDto(
                    review.ReviewID,
                    review.CandidateJobID,
                    review.Reviewer?.FirstName ?? "Unknown",
                    review.Decision,
                    review.Comment,
                    review.RecruiterDecision,
                    review.ReviewedAt,
                    skills
                ));
            }

            return response;
        }


    }
}
