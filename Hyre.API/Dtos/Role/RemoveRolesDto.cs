namespace Hyre.API.Dtos.Role
{
    public record RemoveRolesDto(string UserEmail, List<string> RoleNames);

}
