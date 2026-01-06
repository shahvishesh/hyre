using Hyre.API.Dtos.Feedback;
using Hyre.API.Dtos.RecruiterRoundDecesion;
using Hyre.API.Interfaces.RecruiterFeedback;
using Hyre.API.Models;

namespace Hyre.API.Services
{
    public class RecruiterDecisionService : IRecruiterDecisionService
    {
        private readonly IRecruiterDecisionRepository _repo;

        public RecruiterDecisionService(IRecruiterDecisionRepository repo)
        {
            _repo = repo;
        }

        public async Task ApplyDecisionAsync(
            RecruiterRoundDecisionDto dto, string recruiterId)
        {
            var round = await _repo.GetRoundAsync(dto.CandidateRoundID)
                ?? throw new Exception("Interview round not found.");

            if (round.Status != "Completed")
                throw new InvalidOperationException(
                    "Recruiter decision allowed only after round completion.");

            if (round.RecruiterDecision != null)
                throw new InvalidOperationException(
                    "Decision already applied for this round.");

            round.RecruiterDecisionBy = recruiterId;
            round.RecruiterDecisionAt = DateTime.UtcNow;

            switch (dto.Decision)
            {
                case "Reject":
                    round.RecruiterDecision = "Rejected";
                    await RejectCandidate(round);
                    break;

                case "MoveNext":
                    round.RecruiterDecision = "Approved";
                    await MoveToNextRound(round);
                    break;

                case "Shortlist":
                    round.RecruiterDecision = "Shortlisted";
                    await ShortlistCandidate(round);
                    break;

                default:
                    throw new InvalidOperationException("Invalid decision value.");
            }

            await _repo.SaveAsync();
        }

        private async Task RejectCandidate(CandidateInterviewRound round)
        {
            var futureRounds = await _repo.GetFutureRoundsAsync(
                round.CandidateID, round.JobID, round.SequenceNo);

            foreach (var r in futureRounds)
            {
                r.Status = "Cancelled";
            }

            var candidateJob = await _repo.GetCandidateJobAsync(round.CandidateID, round.JobID);
            if (candidateJob != null)
            {
                candidateJob.Stage = "Rejected";
            }
        }


        private async Task MoveToNextRound(CandidateInterviewRound round)
        {
            var nextRound = await _repo.GetNextRoundAsync(
                round.CandidateID, round.JobID, round.SequenceNo);

            if (nextRound == null)
                throw new InvalidOperationException(
                    "No next round found. Use Shortlist instead.");

            if (nextRound.Status == "Blocked")
                nextRound.Status = "Scheduled";

            var candidateJob = await _repo.GetCandidateJobAsync(round.CandidateID, round.JobID);
            if (candidateJob != null)
            {
                candidateJob.Stage = "Interview";
            }
        }


        private async Task ShortlistCandidate(CandidateInterviewRound round)
        {
            var nextRound = await _repo.GetNextRoundAsync(
                round.CandidateID, round.JobID, round.SequenceNo);

            if (nextRound != null)
                throw new InvalidOperationException(
                    "Candidate has more rounds. Cannot shortlist yet.");

            var candidateJob = await _repo.GetCandidateJobAsync(round.CandidateID, round.JobID);
            if (candidateJob != null)
            {
                candidateJob.Stage = "Shortlisted"; 
            }
        }

        public async Task<RoundDetailDto?> GetNextRoundDetailAsync(int candidateId, int jobId, int currentSequenceNo)
        {
            var nextRound = await _repo.GetNextRoundDetailAsync(candidateId, jobId, currentSequenceNo);

            if (nextRound == null)
                return null;

            // Build panel members list (only if it's a panel round)
            List<PanelMemberDetailDto>? panelMembers = null;
            if (nextRound.IsPanelRound && nextRound.PanelMembers != null && nextRound.PanelMembers.Any())
            {
                panelMembers = nextRound.PanelMembers
                    .Where(pm => pm.Interviewer != null)
                    .Select(pm => new PanelMemberDetailDto(
                        pm.InterviewerID,
                        pm.Interviewer.FirstName,
                        pm.Interviewer.LastName,
                        pm.Interviewer.Email
                    ))
                    .ToList();
            }

            // Build interviewer details (only for non-panel rounds)
            InterviewerDetailDto? interviewer = null;
            if (!nextRound.IsPanelRound && nextRound.Interviewer != null)
            {
                interviewer = new InterviewerDetailDto(
                    nextRound.InterviewerID!,
                    nextRound.Interviewer.FirstName,
                    nextRound.Interviewer.LastName,
                    nextRound.Interviewer.Email
                );
            }

            return new RoundDetailDto(
                nextRound.CandidateRoundID,
                nextRound.CandidateID,
                $"{nextRound.Candidate.FirstName} {nextRound.Candidate.LastName}",
                nextRound.JobID,
                nextRound.Job.Title,
                nextRound.SequenceNo,
                nextRound.RoundName,
                nextRound.RoundType ?? string.Empty,
                nextRound.IsPanelRound,
                nextRound.ScheduledDate,
                nextRound.StartTime,
                nextRound.DurationMinutes,
                nextRound.InterviewMode,
                nextRound.Status,
                nextRound.MeetingLink,
                nextRound.CreatedAt,
                nextRound.UpdatedAt,
                nextRound.RecruiterDecision,
                nextRound.RecruiterDecisionAt,
                panelMembers,
                interviewer
            );
        }

        public async Task<RecruiterDecisionResultDto?> GetRecruiterDecisionAsync(int roundId)
        {
            var round = await _repo.GetRoundWithDecisionDetailsAsync(roundId);

            if (round == null)
                return null;

            return new RecruiterDecisionResultDto(
                round.CandidateRoundID,
                round.CandidateID,
                $"{round.Candidate.FirstName} {round.Candidate.LastName}",
                round.JobID,
                round.Job.Title,
                round.RoundName,
                round.RoundType ?? string.Empty,
                round.RecruiterDecision,
                round.RecruiterDecisionAt,
                round.RecruiterDecisionBy,
                $"{round.Recruiter.FirstName} {round.Recruiter.LastName}"
            );
        }

    }
}
