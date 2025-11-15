using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Hyre.API.Models
{
    public class JobInterviewRoundTemplate
    {
        [Key]
        public int RoundTemplateID { get; set; }

        [Required]
        public int JobID { get; set; }

        [ForeignKey(nameof(JobID))]
        public Job Job { get; set; }

        [Required]
        public int SequenceNo { get; set; } 

        [Required, MaxLength(100)]
        public string RoundName { get; set; } 

        [MaxLength(50)]
        public string RoundType { get; set; } // "Technical", "HR", "Panel"

        public int DurationMinutes { get; set; } // 30, 45, 60, 90, 120

        [MaxLength(50)]
        public string InterviewMode { get; set; } = "Online"; // "Online", "InPerson"

        public bool IsPanelRound { get; set; } = false;
    }
}
