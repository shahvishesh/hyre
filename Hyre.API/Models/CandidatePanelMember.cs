using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Hyre.API.Models
{
    public class CandidatePanelMember
    {
        [Key]
        public int PanelMemberID { get; set; }

        [Required]
        public int CandidateRoundID { get; set; }

        [ForeignKey(nameof(CandidateRoundID))]
        public CandidateInterviewRound CandidateRound { get; set; }

        [Required]
        public string InterviewerID { get; set; } // ApplicationUser ID

        [ForeignKey(nameof(InterviewerID))]
        public ApplicationUser Interviewer { get; set; }
    }
}
