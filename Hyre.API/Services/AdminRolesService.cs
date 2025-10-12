using Hyre.API.Dtos.Role;
using Hyre.API.Interfaces.Role;
using Hyre.API.Models;
using Microsoft.AspNetCore.Identity;

namespace Hyre.API.Services
{
    public class AdminRolesService
    {
        private readonly IAdminRolesRepository _adminRepository;
        private readonly UserManager<ApplicationUser> _userManager;

        public AdminRolesService(IAdminRolesRepository adminRepository, UserManager<ApplicationUser> userManager)
        {
            _adminRepository = adminRepository;
            _userManager = userManager;
        }

        public async Task<IEnumerable<UserRoleDto>> GetAllUsersAsync()
            => await _adminRepository.GetAllUsersAsync();

        public async Task<UserRoleDto?> GetUserRolesAsync(string email)
            => await _adminRepository.GetUserByEmailAsync(email);

        public async Task<string> AssignMultipleRolesAsync(AssignRolesDto dto)
        {
            var user = await _userManager.FindByEmailAsync(dto.UserEmail);
            if (user == null)
                throw new Exception("User not found");

            var assignedRoles = new List<string>();
            var skippedRoles = new List<string>();

            foreach (var roleName in dto.RoleNames)
            {
                if (!await _adminRepository.RoleExistsAsync(roleName))
                {
                    skippedRoles.Add($"{roleName} (not found)");
                    continue;
                }

                if (!await _userManager.IsInRoleAsync(user, roleName))
                {
                    var success = await _adminRepository.AddUserToRoleAsync(user, roleName);
                    if (success)
                        assignedRoles.Add(roleName);
                    else
                        skippedRoles.Add($"{roleName} (error)");
                }
                else
                {
                    skippedRoles.Add($"{roleName} (already assigned)");
                }
            }

            return $" Assigned: {string.Join(", ", assignedRoles)} | Skipped: {string.Join(", ", skippedRoles)}";
        }

        public async Task<string> RemoveMultipleRolesAsync(RemoveRolesDto dto)
        {
            var user = await _userManager.FindByEmailAsync(dto.UserEmail);
            if (user == null)
                throw new Exception("User not found");

            var removedRoles = new List<string>();
            var skippedRoles = new List<string>();

            foreach (var roleName in dto.RoleNames)
            {
                if (!await _userManager.IsInRoleAsync(user, roleName))
                {
                    skippedRoles.Add($"{roleName} (not assigned)");
                    continue;
                }

                var success = await _adminRepository.RemoveUserFromRoleAsync(user, roleName);
                if (success)
                    removedRoles.Add(roleName);
                else
                    skippedRoles.Add($"{roleName} (error)");
            }

            return $" Removed: {string.Join(", ", removedRoles)} | Skipped: {string.Join(", ", skippedRoles)}";
        }
    }
}
