namespace Hyre.API.Dtos.CandidateMatching
{
    public record LinkedCandidateDto(
        int CandidateJobID,
        int CandidateID,
        string FullName,
        string? Email,
        string? Phone,
        decimal? ExperienceYears,
        string Stage,
        DateTime CreatedAt,
        DateTime UpdatedAt,
        List<CandidateSkillSummaryDto> Skills
    );

    public record CandidateSkillSummaryDto(
        int SkillID,
        string SkillName,
        decimal? YearsOfExperience
    );

    public record LinkedCandidatesResponseDto(
        int JobID,
        string JobTitle,
        List<LinkedCandidateDto> LinkedCandidates,
        int TotalCount
    );
}