namespace Hyre.API.Dtos.Role
{
    public record UserRoleDto(string UserId, string Email, string FullName, List<string> Roles);
}
