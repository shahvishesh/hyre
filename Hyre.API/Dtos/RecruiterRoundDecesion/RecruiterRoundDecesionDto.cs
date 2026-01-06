using Hyre.API.Dtos.Feedback;

namespace Hyre.API.Dtos.RecruiterRoundDecesion
{
    public enum RecruiterDecisionState
    {
        Pending,    // RecruiterDecision == null
        Decided,    // RecruiterDecision != null
        All         // All rounds regardless of decision state
    }

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

    public record RecruiterRoundDecisionDto(
        int CandidateRoundID,
        string Decision // "Reject" | "MoveNext" | "Shortlist"
    );

    public record PendingRecruiterDecisionDto(
        int CandidateRoundID,
        int CandidateID,
        string CandidateName,
        int JobID,
        string JobTitle,
        string RoundName,
        string RoundType,
        DateTime InterviewDate,
        string Status
    );

    public record RecruiterDecisionResultDto(
        int CandidateRoundID,
        int CandidateID,
        string CandidateName,
        int JobID,
        string JobTitle,
        string RoundName,
        string RoundType,
        string? RecruiterDecision,
        DateTime? RecruiterDecisionAt,
        string? RecruiterDecisionBy,
        string RecruiterName
    );
}
