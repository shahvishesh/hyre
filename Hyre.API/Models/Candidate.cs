using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Hyre.API.Models
{
    public class Candidate
    {
        [Key]
        public int CandidateID { get; set; }

        [Required, MaxLength(100)]
        public string FirstName { get; set; }

        [MaxLength(100)]
        public string? LastName { get; set; }

        [MaxLength(150), EmailAddress]
        public string? Email { get; set; }

        [MaxLength(20)]
        public string? Phone { get; set; }

        [Precision(5, 2)]
        public decimal? ExperienceYears { get; set; }

        [MaxLength(255)]
        public string? ResumePath { get; set; }

        [MaxLength(50)]
        public string Status { get; set; } = "Active";

        public string? UserId { get; set; }

        [ForeignKey(nameof(UserId))]
        public ApplicationUser? User { get; set; }

        public String? CreatedBy { get; set; }

        [ForeignKey(nameof(CreatedBy))]
        public ApplicationUser? CreatedByUser { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public DateTime UpdatedAt { get; set; } = DateTime.Now;

        public ICollection<CandidateSkill> CandidateSkills { get; set; } = new List<CandidateSkill>();
    }
}
