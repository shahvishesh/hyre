namespace Hyre.API.Dtos.Feedback
{
    public record SkillRatingDto(
        int SkillID,
        int Rating
    );

    public record SubmitFeedbackDto(
        int CandidateRoundID,
        string? OverallComment,
        List<SkillRatingDto> SkillRatings
    );

    public record FeedbackResponseDto(
        int FeedbackID,
        int CandidateRoundID,
        string InterviewerName,
        string? OverallComment,
        DateTime SubmittedAt,
        List<SkillRatingDto> SkillRatings
    );

    public record PendingFeedbackDto(
        int CandidateRoundID,
        int CandidateID,
        string CandidateName,
        int JobID,
        string JobTitle,
        string RoundName,
        string RoundType,
        DateTime InterviewDate
    );

    public record CompletedFeedbackDto(
        int CandidateRoundID,
        int CandidateID,
        string CandidateName,
        int JobID,
        string JobTitle,
        string RoundName,
        string RoundType,
        DateTime InterviewDate
    );
}
