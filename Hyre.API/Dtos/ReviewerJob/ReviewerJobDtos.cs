namespace Hyre.API.Dtos.ReviewerJob
{
    public class ReviewerJobDtos
    {
        public record AssignReviewerDto(int JobId, List<string> ReviewerIds);

        public record JobReviewerDto(int JobReviewerId, int JobId, string ReviewerId, string ReviewerName, DateTime AssignedAt);

        public record JobResponseDto(
            int JobID,
            string Title,
            string? Description,
            int? MinExperience,
            int? MaxExperience,
            string CompanyName,
            string? Location,
            string JobType,
            string WorkplaceType,
            string Status,
            DateTime CreatedAt,
            List<JobSkillDetailDto> Skills);

        public record JobSkillDetailDto(
            int SkillID,
            string SkillName,
            string SkillType);
    }
}
