namespace Hyre.API.Dtos.Candidate
{
    public record CreateCandidateDto(
        string FirstName,
        string? LastName,
        string? Email,
        string? Phone,
        decimal? ExperienceYears,
        List<CreateCandidateSkillDto>? Skills
    );
}
