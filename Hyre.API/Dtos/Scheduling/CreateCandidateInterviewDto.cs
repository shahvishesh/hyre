namespace Hyre.API.Dtos.Scheduling
{
    public record RoundCreateDto(
        int SequenceNo,
        string RoundName,
        bool IsPanelRound,
        List<string> InterviewerIds, // single or multiple (for single it's a list with one id)
        string InterviewMode,
        DateTime? ScheduledDate,     
        TimeSpan? StartTime,
        int DurationMinutes
    );

    public record CreateCandidateInterviewDto(
        int CandidateId,
        int JobId,
        List<RoundCreateDto> TechnicalRounds,
        RoundCreateDto? HrRound 
    );

    public record ScheduleResultDto(
        int RoundId,
        int CandidateId,
        DateTime ScheduledStart, 
        DateTime ScheduledEnd,
        string MeetingLink
    );
}
