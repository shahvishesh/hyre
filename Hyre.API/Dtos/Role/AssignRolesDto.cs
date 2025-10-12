namespace Hyre.API.Dtos.Role
{
    public record AssignRolesDto(string UserEmail, List<string> RoleNames);
}
