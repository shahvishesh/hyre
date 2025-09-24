using System.ComponentModel.DataAnnotations;

namespace Hyre.API.Models
{
    public class Candidate
    {
        [Key]
        public int CandidateID { get; set; }

        [Required]
        [MaxLength(50)]
        public string FirstName { get; set; }

        [MaxLength(50)]
        public string LastName { get; set; }

        [MaxLength(100)]
        public string Email { get; set; }

        [MaxLength(20)]
        public string Phone { get; set; }

        [MaxLength(255)]
        public string ResumePath { get; set; }

        [MaxLength(50)]
        public string Status { get; set; } = "New";
    }
}
