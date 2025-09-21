using System.ComponentModel.DataAnnotations;

namespace Hyre.API.Models
{
    public class Role
    {
        [Key]
        public int RoleID { get; set; }

        [Required]
        [MaxLength(50)]
        public string RoleName { get; set; }
    }
}
