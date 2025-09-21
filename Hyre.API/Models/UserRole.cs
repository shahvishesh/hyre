using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Hyre.API.Models
{
    public class UserRole
    {
        [Key]
        public int UserRoleID { get; set; }

        [Required]
        public int UserID { get; set; }

        [ForeignKey(nameof(UserID))]
        public User User { get; set; }

        [Required]
        public int RoleID { get; set; }

        [ForeignKey(nameof(RoleID))]
        public Role Role { get; set; }
    }
}
