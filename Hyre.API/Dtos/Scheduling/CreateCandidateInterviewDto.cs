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
        int DurationMinutes,
        string RoundType
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

    public record CandidateRoundDto(
        int? CandidateRoundId,
        int SequenceNo,
        string RoundName,
        string RoundType,
        bool IsPanelRound,
        List<string> InterviewerIds,
        string InterviewMode,
        DateTime? ScheduledDate,
        TimeSpan? StartTime,
        int DurationMinutes,
        string Status,
        string? ClientTempId 
    );

    public record CandidateRoundsUpdateDto(
        int CandidateId,
        int JobId,
        List<CandidateRoundDto> Rounds
    );

    public record UpsertRoundResponseDto(
        List<CandidateRoundDto> Rounds,
        Dictionary<string, int> TempIdMap
    );

    public record JobScheduleStateDto(
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

}
