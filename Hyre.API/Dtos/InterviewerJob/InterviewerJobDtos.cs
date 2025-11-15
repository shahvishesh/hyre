namespace Hyre.API.Dtos.InterviewerJob
{
    public record AssignInterviewersDto(
        int JobID,
        List<string> InterviewerIDs,
        string? Role,
        string? SkillArea
    );

    public record RemoveInterviewerDto(
        int JobID,
        string InterviewerID
    );

    public record JobInterviewerDto(
        string InterviewerID,
        string FirstName,
        string LastName,
        string Email,
        string? Role,
        string? SkillArea,
        DateTime AssignedAt
    );
}
