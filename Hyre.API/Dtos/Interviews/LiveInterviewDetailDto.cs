namespace Hyre.API.Dtos.Interviews
{
    public record LiveInterviewDetailDto(
        // Round Details
        int CandidateRoundID,
        string RoundName,
        string RoundType,
        bool IsPanelRound,
        DateTime? ScheduledStart,
        DateTime? ScheduledEnd,
        string Status,
        string? InterviewMode,
        string? MeetingLink,
        
        // Candidate Details
        CandidateDetailDto Candidate,
        
        // Job Details
        JobDetailDto Job,
        
        // Panel Members (only populated if IsPanelRound is true)
        List<PanelMemberDto>? PanelMembers
    );

    public record CandidateDetailDto(
        int CandidateID,
        string FirstName,
        string? LastName,
        string? Email,
        string? Phone,
        decimal? ExperienceYears,
        string? ResumePath,
        string Status,
        List<CandidateSkillDto> Skills
    );

    public record JobDetailDto(
        int JobID,
        string Title,
        string Description,
        int? MinExperience,
        int? MaxExperience,
        string CompanyName,
        string Location,
        string JobType,
        string WorkplaceType,
        string Status,
        List<JobSkillDto> Skills
    );

    public record PanelMemberDto(
        string InterviewerID,
        string FirstName,
        string LastName,
        string? Email
    );

    public record CandidateSkillDto(
        int SkillID,
        string SkillName,
        decimal? YearsOfExperience
    );

    public record JobSkillDto(
        int SkillID,
        string SkillName,
        string SkillType
    );
}