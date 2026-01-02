using Hyre.API.Dtos.Role;
using Hyre.API.Models;

namespace Hyre.API.Interfaces.Role
{
    public interface IAdminRolesRepository
    {
        Task<IEnumerable<UserRoleDto>> GetAllUsersAsync();
        Task<UserRoleDto?> GetUserByEmailAsync(string email);
        Task<bool> RoleExistsAsync(string roleName);
        Task<bool> AddUserToRoleAsync(ApplicationUser user, string roleName);
        Task<bool> RemoveUserFromRoleAsync(ApplicationUser user, string roleName);
        Task<IList<string>> GetUserRolesAsync(ApplicationUser user);
        Task<List<RoleDto>> GetAllRolesAsync(); 
    }
}
