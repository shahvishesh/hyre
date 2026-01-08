using Hyre.API.Models;

namespace Hyre.API.Dtos.CandidateReview
{
    public class ReviewDtos
    {

        public record CreateReviewDto(int CandidateJobID, string? Comment, string Decision, List<ReviewedSkillDto>? Skills);
        public record UpdateReviewDto(int ReviewID, string? Comment, string Decision, List<ReviewedSkillDto>? Skills);


        public record AddCommentDto(int ReviewID, string CommentText);
        public record CommentResponseDto(string name, string CommentText, DateTime CommentedAt);

        public record RecruiterDecisionDto(int ReviewID, string Decision);

        public record ReviewedSkillDto(
            int SkillId,
            bool IsVerified,
            decimal? VerifiedYearsOfExperience
         );

        public record ReviewResponseDto(
            int ReviewID,
            int CandidateJobID,
            string ReviewerName,
            string Decision,
            string? Comment,
            string? RecruiterDecision,
            DateTime ReviewedAt,
            List<ReviewedSkillDto> Skills,
            List<CommentResponseDto> Comments
        );

        public record ReviewerJobDto(
            int JobID,
            string Title,
            string Description,
            string CompanyName,
            string Location,
            string JobType,
            string WorkplaceType,
            string Status,
            int? MinExperience,
            int? MaxExperience,
            DateTime CreatedAt,
            int PendingProfilesCount
        );

        public record ReviewerResponseDto(
            int ReviewID,
            int CandidateJobID,
            string ReviewerName,
            string Decision,
            string? Comment,
            string? RecruiterDecision,
            DateTime ReviewedAt,
            List<ReviewedSkillDto> Skills
        );
    }
}
