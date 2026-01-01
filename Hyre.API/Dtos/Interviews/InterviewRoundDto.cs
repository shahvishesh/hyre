namespace Hyre.API.Dtos.Interviews
{
    public record InterviewRoundDto(
        int CandidateRoundID,
        int CandidateID,
        string CandidateName,
        int JobID,
        string JobTitle,
        string RoundName,
        string RoundType,
        bool IsPanelRound,
        DateTime? ScheduledStart,
        DateTime? ScheduledEnd,
        string Status,
        string? InterviewMode,
        string? MeetingLink
    );
}
