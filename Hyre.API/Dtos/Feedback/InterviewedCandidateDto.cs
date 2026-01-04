namespace Hyre.API.Dtos.Feedback
{
    public record InterviewedCandidateDto(
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

    public record CandidateSkillDto(
        int SkillID,
        string SkillName,
        decimal? YearsOfExperience
    );
}