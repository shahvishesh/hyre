using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Hyre.API.Models
{
    public class CandidateSkill
    {
        [Key]
        public int CandidateSkillID { get; set; }

        [ForeignKey(nameof(Candidate))]
        public int CandidateID { get; set; }
        public Candidate Candidate { get; set; }

        [ForeignKey(nameof(Skill))]
        public int SkillID { get; set; }
        public Skill Skill { get; set; }

        [Precision(4, 1)]
        public decimal? YearsOfExperience { get; set; }

        public string? AddedBy { get; set; }

        [ForeignKey(nameof(AddedBy))]
        public ApplicationUser? AddedByUser { get; set; }

        public DateTime AddedAt { get; set; } = DateTime.Now;
    }
}
