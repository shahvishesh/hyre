using Hyre.API.Dtos.Feedback;

namespace Hyre.API.Dtos.RecruiterRoundDecesion
{
    public record SkillAggregateDto(
        int SkillID,
        string SkillName,
        double AverageRating,
        int ReviewerCount
    );

    public record InterviewerFeedbackDto(
        string InterviewerId,
        string InterviewerName,
        string? OverallComment,
        DateTime SubmittedAt,
        List<SkillRatingDto> SkillRatings
    );

    public record RoundAggregatedFeedbackDto(
        int CandidateRoundID,
        string RoundName,
        string RoundType,
        string Status,
        int TotalInterviewers,
        int FeedbackSubmitted,
        List<SkillAggregateDto> SkillAggregates,
        List<InterviewerFeedbackDto> IndividualFeedbacks
    );
}
