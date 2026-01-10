namespace Hyre.API.Dtos.InterviewerJob
{
    public record AssignInterviewersDto(
        int JobID,
        List<string> InterviewerIDs,
        string? Role,
        string? SkillArea
    );

    // New DTOs for the updated method
    public record AssignInterviewersV2Dto(
        int JobID,
        List<InterviewerAssignmentDto> Assignments
    );

    public record InterviewerAssignmentDto(
        string InterviewerID,
        string InterviewRole   // Technical / HR / Managerial
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

    public record InterviewerJobResponseDto(
        int JobID,
        string Title,
        string? Description,
        int? MinExperience,
        int? MaxExperience,
        string CompanyName,
        string? Location,
        string JobType,
        string WorkplaceType,
        string Status,
        DateTime CreatedAt,
        List<JobSkillDetailDto> Skills
    );

    public record JobSkillDetailDto(
        int SkillID,
        string SkillName,
        string SkillType
    );

    public record EmployeeDetailDto(
        int EmployeeId,
        string UserId,
        string FullName,
        string Email,
        string? Designation,
        List<string> SystemRoles
    );

    // New DTOs for assigned interviewers
    public record AssignedInterviewerDto(
        string InterviewerID,
        string FullName,
        string Email,
        string? Designation,
        List<string> SystemRoles,
        string InterviewRole,   // Technical / HR / Managerial
        DateTime AssignedAt
    );

    public record JobAssignedInterviewersDto(
        int JobID,
        List<AssignedInterviewerDto> Interviewers
    );
}
