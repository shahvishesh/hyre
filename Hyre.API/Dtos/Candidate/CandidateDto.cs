namespace Hyre.API.Dtos.Candidate
{
    public record CandidateDto(
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
}
