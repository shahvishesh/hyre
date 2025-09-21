using System.ComponentModel.DataAnnotations;

namespace Hyre.API.Models
{
    public class Skill
    {
        [Key]
        public int SkillID { get; set; }

        [Required]
        [MaxLength(100)]
        public string SkillName { get; set; }
    }
}
