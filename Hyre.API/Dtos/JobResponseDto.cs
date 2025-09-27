namespace Hyre.API.Dtos
{
    public record JobResponseDto(
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
}
