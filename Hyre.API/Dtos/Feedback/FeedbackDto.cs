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

    public record RoundDetailDto(
        int CandidateRoundID,
        int CandidateID,
        string CandidateName,
        int JobID,
        string JobTitle,
        int SequenceNo,
        string RoundName,
        string RoundType,
        bool IsPanelRound,
        DateTime? ScheduledDate,
        TimeSpan? StartTime,
        int? DurationMinutes,
        string? InterviewMode,
        string Status,
        string? MeetingLink,
        DateTime CreatedAt,
        DateTime UpdatedAt,
        string? RecruiterDecision,
        DateTime? RecruiterDecisionAt,
        List<PanelMemberDetailDto>? PanelMembers,
        InterviewerDetailDto? Interviewer
    );

    public record PanelMemberDetailDto(
        string InterviewerID,
        string FirstName,
        string LastName,
        string? Email
    );

    public record InterviewerDetailDto(
        string InterviewerID,
        string FirstName,
        string LastName,
        string? Email
    );

    public record SimpleFeedbackDto(
        int CandidateRoundID,
        string? OverallComment,
        List<SkillRatingDto> SkillRatings
    );
}
