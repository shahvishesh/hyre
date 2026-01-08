using Hyre.API.Data;
using Hyre.API.Dtos.Feedback;
using Hyre.API.Exceptions;
using Hyre.API.Interfaces;
using Hyre.API.Interfaces.CandidateReview;
using Hyre.API.Interfaces.Candidates;
using Hyre.API.Interfaces.ReviewerJob;
using Hyre.API.Models;
using Microsoft.EntityFrameworkCore;
using static Hyre.API.Dtos.CandidateReview.ReviewDtos;

namespace Hyre.API.Services
{
    public class CandidateReviewService : ICandidateReviewService
    {
        private readonly ApplicationDbContext _context;
        private readonly IJobService _jobService;
        private readonly ICandidateRepository _candidateRepository;
        private readonly IJobReviewerRepository _jobReviewerRepository;
        private readonly IWebHostEnvironment _env;


        public CandidateReviewService(ApplicationDbContext context, IJobService jobService, ICandidateRepository candidateRepository, IJobReviewerRepository jobReviewerRepository, IWebHostEnvironment env)
        {
            _context = context;
            _jobService = jobService;
            _candidateRepository = candidateRepository;
            _jobReviewerRepository = jobReviewerRepository;
            _env = env;
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

            review.Comment = dto.Comment ?? review.Comment;
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
                .Include(x => x.Comments).ThenInclude(c => c.Commenter)
                .FirstOrDefaultAsync(x => x.ReviewID == reviewId);
            if (r == null) throw new Exception("Review not found.");

            var skills = r.SkillReviews.Select(sr => new ReviewedSkillDto(
                sr.SkillId,
                sr.IsVerified,
                sr.VerifiedYearsOfExperience
            )).ToList();

            var comments = r.Comments.Select(c => new CommentResponseDto(
                $"{c.Commenter?.FirstName} {c.Commenter?.LastName}" ?? "Unknown",
                c.CommentText,
                c.CommentedAt
            )).ToList();

            return new ReviewResponseDto(
                r.ReviewID,
                r.CandidateJobID,
                r.Reviewer?.FirstName ?? "Unknown",
                r.Decision,
                r.Comment,
                r.RecruiterDecision,
                r.ReviewedAt,
                skills,
                comments
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
            var candidateJob = await _context.CandidateJobs.FindAsync(review.CandidateJobID) ?? throw new Exception("Candidate-job link not found.");

            if (dto.Decision == "Shortlisted")
            {
                candidateJob.Stage = "Interview";

                await CloneInterviewRoundsAsync(candidateJob.CandidateID, candidateJob.JobID, recruiterId);
            }
            else if (dto.Decision == "Rejected")
            {
                candidateJob.Stage = "Rejected";
            }

            await _context.SaveChangesAsync();
        }

        private async Task CloneInterviewRoundsAsync(int candidateId, int jobId, string recruiterId)
        {
            bool alreadyHasRounds = await _context.CandidateInterviewRounds
                .AnyAsync(r => r.CandidateID == candidateId && r.JobID == jobId);

            if (alreadyHasRounds)
                return;


            var templates = await _context.JobInterviewRoundTemplates
                .Where(t => t.JobID == jobId)
                .OrderBy(t => t.SequenceNo)
                .ToListAsync();

            if (!templates.Any())
                return;  

            foreach (var t in templates)
            {
                var round = new CandidateInterviewRound
                {
                    CandidateID = candidateId,
                    JobID = jobId,
                    SequenceNo = t.SequenceNo,
                    RoundName = t.RoundName,
                    RoundType = t.RoundType,
                    DurationMinutes = t.DurationMinutes,
                    InterviewMode = t.InterviewMode,
                    IsPanelRound = t.IsPanelRound,
                    RecruiterID = recruiterId,
                    Status = "Pending",
                    CreatedAt = DateTime.Now,
                    UpdatedAt = DateTime.Now
                };

                _context.CandidateInterviewRounds.Add(round);
            }

            await _context.SaveChangesAsync();
        }


        public async Task<IEnumerable<ReviewResponseDto>> GetReviewsByJobAsync(int jobId)
        {
            var job = await _jobService.GetJobByIdAsync(jobId);
            if (job == null) throw new Exception("Job not found.");

            var reviews = await _context.CandidateReviews
                .Include(r => r.Reviewer)
                .Include(r => r.Recruiter)
                .Include(r => r.CandidateJob)
                .Include(r => r.SkillReviews).ThenInclude(sr => sr.Skill)
                .Include(r => r.Comments).ThenInclude(c => c.Commenter)
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

                var comments = review.Comments.Select(c => new CommentResponseDto(
                    $"{c.Commenter?.FirstName} {c.Commenter?.LastName}" ?? "Unknown",
                    c.CommentText,
                    c.CommentedAt
                )).ToList();

                response.Add(new ReviewResponseDto(
                    review.ReviewID,
                    review.CandidateJobID,
                    review.Reviewer?.FirstName ?? "Unknown",
                    review.Decision,
                    review.Comment,
                    review.RecruiterDecision,
                    review.ReviewedAt,
                    skills,
                    comments
                ));
            }

            return response;
        }

        public async Task<byte[]> GetCandidateResumeAsync(
            int candidateId, string requesterId, int jobId, IEnumerable<string> userRoles)
        {
            var candidate = await _candidateRepository.GetCandidateByIdAsync(candidateId);
            if (candidate == null)
                throw new Exception("Candidate not found.");
            
            var job = await _jobService.GetJobByIdAsync(jobId);
            if (job == null)
            {
                throw new Exception("Job not found");
            }


            bool isRecruiterOrAdmin = userRoles.Contains("Recruiter") ||
                                      userRoles.Contains("Admin");

            bool isReviewerAssigned = await _jobReviewerRepository
                .IsReviewerAssignedToJobAsync(requesterId, jobId);

            bool isCandidateLinked = await _candidateRepository
                .IsCandidateLinkedToJobAsync(candidateId, jobId);

            if (!isRecruiterOrAdmin)
            {
                if (!isCandidateLinked)
                    throw new ForbiddenAccessException("Candidate is not linked to this job.");

                if (!isReviewerAssigned)
                    throw new ForbiddenAccessException("You are not assigned to this job.");
            }
            if (string.IsNullOrEmpty(candidate.ResumePath))
                throw new Exception("No resume uploaded for this candidate.");

            var resumePath = Path.Combine(_env.ContentRootPath, "PrivateFiles", candidate.ResumePath);
            if (!File.Exists(resumePath))
                throw new Exception("Resume file not found.");

            var fileBytes = await File.ReadAllBytesAsync(resumePath);
            //var fileName = Path.GetFileName(resumePath);

            return fileBytes;
        }

        public async Task<List<ReviewerJobDto>> GetJobsAssignedToReviewerAsync(string reviewerId)
        {
            var jobsWithCounts = await _context.JobReviewers
                .Where(jr => jr.ReviewerId == reviewerId)
                .Select(jr => new
                {
                    Job = jr.Job,
                    PendingCount = _context.CandidateJobs
                        .Where(cj => cj.JobID == jr.JobId)
                        .Count(cj => !_context.CandidateReviews.Any(cr => cr.CandidateJobID == cj.CandidateJobID))
                })
                .Distinct()
                .ToListAsync();

            return jobsWithCounts.Select(item => new ReviewerJobDto(
                item.Job.JobID,
                item.Job.Title,
                item.Job.Description ?? string.Empty,
                item.Job.CompanyName,
                item.Job.Location ?? string.Empty,
                item.Job.JobType,
                item.Job.WorkplaceType,
                item.Job.Status,
                item.Job.MinExperience,
                item.Job.MaxExperience,
                item.Job.CreatedAt,
                item.PendingCount
            )).ToList();
        }

        public async Task<List<ReviewerJobDto>> GetOpenJobsWithPendingReviewsAsync()
        {
            var openJobsWithCounts = await _context.Jobs
                .Where(j => j.Status == "Open")
                .Select(job => new
                {
                    Job = job,
                    PendingCount = _context.CandidateReviews
                        .Where(cr => _context.CandidateJobs.Any(cj => cj.JobID == job.JobID && cj.CandidateJobID == cr.CandidateJobID))
                        .Count(cr => cr.RecruiterDecision == null)
                })
                .ToListAsync();

            return openJobsWithCounts.Select(item => new ReviewerJobDto(
                item.Job.JobID,
                item.Job.Title,
                item.Job.Description ?? string.Empty,
                item.Job.CompanyName,
                item.Job.Location ?? string.Empty,
                item.Job.JobType,
                item.Job.WorkplaceType,
                item.Job.Status,
                item.Job.MinExperience,
                item.Job.MaxExperience,
                item.Job.CreatedAt,
                item.PendingCount
            )).ToList();
        }

        public async Task<List<InterviewedCandidateDto>> GetCandidatesByRecruitmentStatusAsync(int jobId, string status)
        {
            IQueryable<CandidateReview> reviewQuery = _context.CandidateReviews
                .Include(cr => cr.CandidateJob)
                    .ThenInclude(cj => cj.Candidate)
                    .ThenInclude(c => c.CandidateSkills)
                    .ThenInclude(cs => cs.Skill)
                .Where(cr => cr.CandidateJob.JobID == jobId);

            // Filter based on recruiter decision status
            if (status.ToLower() == "pending")
            {
                reviewQuery = reviewQuery.Where(cr => cr.RecruiterDecision == null);
            }
            else if (status.ToLower() == "completed")
            {
                reviewQuery = reviewQuery.Where(cr => cr.RecruiterDecision != null);
            }
            else
            {
                throw new ArgumentException("Invalid status. Use 'pending' or 'completed'.");
            }

            var reviews = await reviewQuery.ToListAsync();

            return reviews.Select(review => new InterviewedCandidateDto(
                review.CandidateJob.Candidate.CandidateID,
                review.CandidateJob.Candidate.FirstName,
                review.CandidateJob.Candidate.LastName,
                review.CandidateJob.Candidate.Email,
                review.CandidateJob.Candidate.Phone,
                review.CandidateJob.Candidate.ExperienceYears,
                review.CandidateJob.Candidate.ResumePath,
                review.CandidateJob.Candidate.Status,
                review.CandidateJob.Candidate.CandidateSkills?
                    .Where(cs => cs.Skill != null)
                    .Select(cs => new CandidateSkillDto(
                        cs.SkillID,
                        cs.Skill.SkillName,
                        cs.YearsOfExperience
                    ))
                    .ToList() ?? new List<CandidateSkillDto>()
            )).ToList();
        }

        public async Task<ReviewerResponseDto?> GetCandidateReviewForJobAsync(int candidateId, int jobId)
        {
            var candidateJob = await _context.CandidateJobs
                .FirstOrDefaultAsync(cj => cj.CandidateID == candidateId && cj.JobID == jobId);

            if (candidateJob == null)
                return null;

            var review = await _context.CandidateReviews
                .Include(r => r.Reviewer)
                .Include(r => r.SkillReviews)
                    .ThenInclude(sr => sr.Skill)
                .FirstOrDefaultAsync(r => r.CandidateJobID == candidateJob.CandidateJobID);

            if (review == null)
                return null;

            var skills = review.SkillReviews.Select(sr => new ReviewedSkillDto(
                sr.SkillId,
                sr.IsVerified,
                sr.VerifiedYearsOfExperience
            )).ToList();

            return new ReviewerResponseDto(
                review.ReviewID,
                review.CandidateJobID,
                $"{review.Reviewer.FirstName} {review.Reviewer.LastName}",
                review.Decision,
                review.Comment,
                review.RecruiterDecision,
                review.ReviewedAt,
                skills
            );
        }
    }
}
