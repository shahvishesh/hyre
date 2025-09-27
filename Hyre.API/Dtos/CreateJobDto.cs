namespace Hyre.API.Dtos
{
    public record CreateJobDto(
    string Title,
    string? Description,
    int? MinExperience,
    int? MaxExperience,
    string CompanyName,
    string? Location,
    string JobType,
    string WorkplaceType,
    List<JobSkillDto> Skills
);
}
