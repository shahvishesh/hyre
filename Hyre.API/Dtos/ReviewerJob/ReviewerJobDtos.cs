namespace Hyre.API.Dtos.ReviewerJob
{
    public class ReviewerJobDtos
    {
        public record AssignReviewerDto(int JobId, List<string> ReviewerIds);

        public record JobReviewerDto(int JobReviewerId, int JobId, string ReviewerId, string ReviewerName, DateTime AssignedAt);

    }
}
