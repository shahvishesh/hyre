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

        public ICollection<JobSkill> JobSkills { get; set; } = new List<JobSkill>();
        public ICollection<CandidateSkill> CandidateSkills { get; set; } = new List<CandidateSkill>();

    }
}
