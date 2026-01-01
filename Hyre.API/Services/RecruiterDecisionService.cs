using Hyre.API.Interfaces.RecruiterFeedback;
using Hyre.API.Dtos.RecruiterRoundDecesion;
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
            RecruiterDecisionDto dto, string recruiterId)
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
        }

        private Task ShortlistCandidate(CandidateInterviewRound round)
        {
            var nextRound = _repo.GetNextRoundAsync(
                round.CandidateID, round.JobID, round.SequenceNo).Result;

            if (nextRound != null)
                throw new InvalidOperationException(
                    "Candidate has more rounds. Cannot shortlist yet.");

            return Task.CompletedTask;
        }
    }
}
