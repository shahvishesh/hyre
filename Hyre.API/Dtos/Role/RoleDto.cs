namespace Hyre.API.Dtos.Role
{
    public record RoleDto(string Id, string Name);
    
    public record RolesResponseDto(List<RoleDto> Roles, int TotalCount);
}