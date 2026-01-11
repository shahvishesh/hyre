using Hyre.API.Dtos.InterviewerJob;
using Hyre.API.Interfaces;
using Hyre.API.Interfaces.InterviewerJob;
using Hyre.API.Models;
using Microsoft.AspNetCore.Identity;

namespace Hyre.API.Services
{
    public class JobInterviewerService : IJobInterviewerService
    {
        private readonly IJobInterviewerRepository _repo;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IJobService _jobService;


        public JobInterviewerService(
            IJobInterviewerRepository repo,
            UserManager<ApplicationUser> userManager,
            IJobService jobService)
        {
            _repo = repo;
            _userManager = userManager;
            _jobService = jobService;
        }

        public async Task AssignInterviewersAsync(AssignInterviewersDto dto, string recruiterId)
        {

            var job = await _jobService.GetJobByIdAsync(dto.JobID);
            if (job == null)
            {
                throw new Exception("Job not found");
            }

            foreach (var interviewerId in dto.InterviewerIDs)
            {
                if (await _repo.ExistsAsync(dto.JobID, interviewerId))
                    continue;

                var user = await _userManager.FindByIdAsync(interviewerId);
                if (user == null)
                {
                    throw new Exception($"Interviewer with ID {interviewerId} not found");
                }

                var entity = new JobInterviewer
                {
                    JobID = dto.JobID,
                    InterviewerID = interviewerId,
                    Role = dto.Role,
                    SkillArea = dto.SkillArea,
                    AssignedBy = recruiterId
                };

                await _repo.AddAsync(entity);
            }
        }

        /*public async Task AssignInterviewersV2Async(AssignInterviewersV2Dto dto, string recruiterId)
        {
            var job = await _jobService.GetJobByIdAsync(dto.JobID);
            if (job == null)
            {
                throw new Exception("Job not found");
            }

            // Get currently assigned interviewers for this job
            var currentAssignments = await _repo.GetAssignedAsync(dto.JobID);
            var currentInterviewerIds = currentAssignments.Select(x => x.InterviewerID).ToHashSet();

            // Get new interviewer IDs from the request
            var newInterviewerIds = dto.Assignments.Select(x => x.InterviewerID).ToHashSet();

            // Find interviewers to remove (currently assigned but not in new list)
            var interviewersToRemove = currentInterviewerIds.Except(newInterviewerIds);

            // Find interviewers to add (in new list but not currently assigned)
            var interviewersToAdd = dto.Assignments.Where(x => !currentInterviewerIds.Contains(x.InterviewerID));

            // Find interviewers to update (in both lists but potentially different interview roles)
            var interviewersToUpdate = dto.Assignments.Where(x => currentInterviewerIds.Contains(x.InterviewerID));

            // Remove interviewers no longer in the list
            foreach (var interviewerId in interviewersToRemove)
            {
                await _repo.RemoveAsync(dto.JobID, interviewerId);
            }

            // Add new interviewers
            foreach (var assignment in interviewersToAdd)
            {
                var user = await _userManager.FindByIdAsync(assignment.InterviewerID);
                if (user == null)
                {
                    throw new Exception($"Interviewer with ID {assignment.InterviewerID} not found");
                }

                var entity = new JobInterviewer
                {
                    JobID = dto.JobID,
                    InterviewerID = assignment.InterviewerID,
                    Role = assignment.InterviewRole,
                    SkillArea = null,
                    AssignedBy = recruiterId
                };

                await _repo.AddAsync(entity);
            }

            // Update existing interviewers if their interview role has changed - NO NEED TO REMOVE/ADD
            foreach (var assignment in interviewersToUpdate)
            {
                var existingAssignment = currentAssignments.First(x => x.InterviewerID == assignment.InterviewerID);
                if (existingAssignment.Role != assignment.InterviewRole)
                {
                    // Just update the existing record instead of remove/add
                    await _repo.UpdateInterviewerRoleAsync(dto.JobID, assignment.InterviewerID, assignment.InterviewRole);
                }
            }
        }*/

        public async Task AssignInterviewersV2Async(AssignInterviewersV2Dto dto, string recruiterId)
        {
            var job = await _jobService.GetJobByIdAsync(dto.JobID);
            if (job == null)
            {
                throw new Exception("Job not found");
            }

            var currentAssignments = await _repo.GetAssignedAsync(dto.JobID);
            var currentInterviewerIds = currentAssignments.Select(x => x.InterviewerID).ToHashSet();

            var newInterviewerIds = dto.Assignments.Select(x => x.InterviewerID).ToHashSet();

            var interviewersToRemove = currentInterviewerIds.Except(newInterviewerIds);

            var interviewersToAdd = dto.Assignments.Where(x => !currentInterviewerIds.Contains(x.InterviewerID));

            var interviewersToUpdate = dto.Assignments.Where(x => currentInterviewerIds.Contains(x.InterviewerID));

            foreach (var interviewerId in interviewersToRemove)
            {
                await _repo.RemoveAsync(dto.JobID, interviewerId);
            }

            foreach (var assignment in interviewersToAdd)
            {
                var user = await _userManager.FindByIdAsync(assignment.InterviewerID);
                if (user == null)
                {
                    throw new Exception($"Interviewer with ID {assignment.InterviewerID} not found");
                }

                var existingInactiveAssignment = await _repo.GetInactiveAssignmentAsync(dto.JobID, assignment.InterviewerID);

                if (existingInactiveAssignment != null)
                {
                    await _repo.ReactivateAssignmentAsync(dto.JobID, assignment.InterviewerID, assignment.InterviewRole, recruiterId);
                }
                else
                {
                    var entity = new JobInterviewer
                    {
                        JobID = dto.JobID,
                        InterviewerID = assignment.InterviewerID,
                        Role = assignment.InterviewRole,
                        SkillArea = null,
                        AssignedBy = recruiterId
                    };

                    await _repo.AddAsync(entity);
                }
            }

            foreach (var assignment in interviewersToUpdate)
            {
                var existingAssignment = currentAssignments.First(x => x.InterviewerID == assignment.InterviewerID);
                if (existingAssignment.Role != assignment.InterviewRole)
                {
                    await _repo.UpdateInterviewerRoleAsync(dto.JobID, assignment.InterviewerID, assignment.InterviewRole);
                }
            }
        }

        public async Task RemoveInterviewerAsync(int jobId, string interviewerId)
        {
            var user = await _userManager.FindByIdAsync(interviewerId);
            if (user == null)
            {
                throw new Exception($"Interviewer with ID {interviewerId} not found");
            }

            var job = await _jobService.GetJobByIdAsync(jobId);
            if (job == null)
            {
                throw new Exception("Job not found");
            }

            await _repo.RemoveAsync(jobId, interviewerId);
        }

        public async Task<List<JobInterviewerDto>> GetAssignedInterviewersAsync(int jobId)
        {
            var job = await _jobService.GetJobByIdAsync(jobId);
            if (job == null)
            {
                throw new Exception("Job not found");
            }

            var list = await _repo.GetAssignedAsync(jobId);

            return list.Select(x => new JobInterviewerDto(
                x.InterviewerID,
                x.Interviewer.FirstName,
                x.Interviewer.LastName,
                x.Interviewer.Email,
                x.Role,
                x.SkillArea,
                x.AssignedAt
            )).ToList();
        }

        public async Task<List<JobInterviewerDto>> GetInterviewersByRoleAsync(int jobId, string role)
        {
            var job = await _jobService.GetJobByIdAsync(jobId);
            if (job == null)
            {
                throw new Exception("Job not found");
            }

            var list = await _repo.GetAssignedByRoleAsync(jobId, role);

            return list.Select(x => new JobInterviewerDto(
                x.InterviewerID,
                x.Interviewer.FirstName,
                x.Interviewer.LastName,
                x.Interviewer.Email,
                x.Role,
                x.SkillArea,
                x.AssignedAt
            )).ToList();
        }

        public async Task<List<InterviewerJobResponseDto>> GetJobsByInterviewerStatusAsync(string status)
        {
            if (string.IsNullOrEmpty(status) ||
                (!status.Equals("pending", StringComparison.OrdinalIgnoreCase) &&
                 !status.Equals("completed", StringComparison.OrdinalIgnoreCase)))
            {
                throw new ArgumentException("Status must be either 'pending' or 'completed'");
            }

            var jobs = await _repo.GetJobsByInterviewerStatusAsync(status);

            return jobs.Select(job => new InterviewerJobResponseDto(
                job.JobID,
                job.Title,
                job.Description,
                job.MinExperience,
                job.MaxExperience,
                job.CompanyName,
                job.Location,
                job.JobType,
                job.WorkplaceType,
                job.Status,
                job.CreatedAt,
                job.JobSkills?.Select(js => new JobSkillDetailDto(
                    js.SkillID,
                    js.Skill?.SkillName ?? string.Empty,
                    js.SkillType
                )).ToList() ?? new List<JobSkillDetailDto>()
            )).ToList();
        }

        public async Task<List<EmployeeDetailDto>> GetEmployeesBySystemRoleAsync(string role)
        {
            var employees =
                await _repo.GetEmployeesByRoleAsync(role);

            var result = new List<EmployeeDetailDto>();

            foreach (var e in employees)
            {
                var roles = await _userManager
                    .GetRolesAsync(e.User!);

                result.Add(new EmployeeDetailDto(
                    e.EmployeeID,
                    e.UserID!,  
                    $"{e.FirstName} {e.LastName}".Trim(),
                    e.User!.Email!,
                    e.Designation,
                    roles.ToList() 
                ));
            }

            return result;
        }

        public async Task<JobAssignedInterviewersDto> GetJobAssignedInterviewersAsync(int jobId)
        {
            var job = await _jobService.GetJobByIdAsync(jobId);
            if (job == null)
            {
                throw new Exception("Job not found");
            }

            var assignedInterviewers = await _repo.GetAssignedAsync(jobId);

            var interviewerDtos = new List<AssignedInterviewerDto>();

            foreach (var assignment in assignedInterviewers)
            {
                var employee = await _repo.GetEmployeeByUserIdAsync(assignment.InterviewerID);

                var roles = await _userManager.GetRolesAsync(assignment.Interviewer);

                interviewerDtos.Add(new AssignedInterviewerDto(
                    assignment.InterviewerID,
                    $"{assignment.Interviewer.FirstName} {assignment.Interviewer.LastName}".Trim(),
                    assignment.Interviewer.Email ?? string.Empty,
                    employee?.Designation,
                    roles.ToList(),
                    assignment.Role ?? string.Empty,
                    assignment.AssignedAt
                ));
            }

            return new JobAssignedInterviewersDto(jobId, interviewerDtos);
        }

        public async Task<JobAssignedInterviewersDto> GetJobAssignedInterviewersByRoleAsync(int jobId, string role)
        {
            var job = await _jobService.GetJobByIdAsync(jobId);
            if (job == null)
            {
                throw new Exception("Job not found");
            }

            if (string.IsNullOrEmpty(role))
            {
                throw new ArgumentException("Role parameter is required");
            }

            if (!role.Equals("Interviewer", StringComparison.OrdinalIgnoreCase) &&
                !role.Equals("HR", StringComparison.OrdinalIgnoreCase))
            {
                throw new ArgumentException("Role must be either 'Interviewer' or 'HR'");
            }

            var assignedInterviewers = await _repo.GetAssignedAsync(jobId);

            var interviewerDtos = new List<AssignedInterviewerDto>();

            foreach (var assignment in assignedInterviewers)
            {
                var userRoles = await _userManager.GetRolesAsync(assignment.Interviewer);

                if (!userRoles.Contains(role, StringComparer.OrdinalIgnoreCase))
                    continue; 

                var employee = await _repo.GetEmployeeByUserIdAsync(assignment.InterviewerID);

                interviewerDtos.Add(new AssignedInterviewerDto(
                    assignment.InterviewerID,
                    $"{assignment.Interviewer.FirstName} {assignment.Interviewer.LastName}".Trim(),
                    assignment.Interviewer.Email ?? string.Empty,
                    employee?.Designation,
                    userRoles.ToList(),
                    assignment.Role ?? string.Empty, // This is the interview role (Technical, HR, Panel)
                    assignment.AssignedAt
                ));
            }

            return new JobAssignedInterviewersDto(jobId, interviewerDtos);
        }

    }
}
