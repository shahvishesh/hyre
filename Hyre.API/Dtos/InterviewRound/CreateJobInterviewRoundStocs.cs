namespace Hyre.API.Dtos.InterviewRound
{
    public record CreateJobInterviewRoundDto(
    int SequenceNo,
    string RoundName,
    string RoundType,       
    int DurationMinutes,
    string InterviewMode,   
    bool IsPanelRound
);

}
