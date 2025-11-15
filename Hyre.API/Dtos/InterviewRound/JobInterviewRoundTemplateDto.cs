namespace Hyre.API.Dtos.InterviewRound
{
    public record JobInterviewRoundTemplateDto(
    int SequenceNo,
    string RoundName,
    string RoundType,
    int DurationMinutes,
    string InterviewMode,
    bool IsPanelRound
);

}
