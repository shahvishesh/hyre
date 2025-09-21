using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Hyre.API.Models
{
    public class JobSkill
    {
        [Key]
        public int JobSkillID { get; set; }

        [Required]
        public int JobID { get; set; }

        [ForeignKey(nameof(JobID))]
        public Job Job { get; set; }

        [Required]
        public int SkillID { get; set; }

        [ForeignKey(nameof(SkillID))]
        public Skill Skill { get; set; }

        [MaxLength(20)]
        [Required]
        public string SkillType { get; set; }
    }
}
