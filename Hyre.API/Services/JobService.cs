using Hyre.API.Data;
using Hyre.API.Dtos;
using Hyre.API.Dtos.InterviewRound;
using Hyre.API.Interfaces;
using Hyre.API.Models;
using System;

namespace Hyre.API.Services
{
    public class JobService : IJobService
    {
        private readonly IJobRepository _jobRepository;
        private readonly ApplicationDbContext _context; 

        public JobService(IJobRepository jobRepository, ApplicationDbContext context)
        {
            _jobRepository = jobRepository;
            _context = context;
        }

        public async Task<JobResponseDto> CreateJobAsync(CreateJobDto dto, String createdByUserId)
        {
            var job = new Job
            {
                Title = dto.Title,
                Description = dto.Description,
                MinExperience = dto.MinExperience,
                MaxExperience = dto.MaxExperience,
                CompanyName = dto.CompanyName,
                Location = dto.Location,
                JobType = dto.JobType,
                WorkplaceType = dto.WorkplaceType,
                Status = "Open",
                CreatedBy = createdByUserId,
                CreatedAt = DateTime.Now,
                UpdatedAt = DateTime.Now
            };

            foreach (var skillDto in dto.Skills)
            {
                var skill = await _context.Skills.FindAsync(skillDto.SkillID);
                if (skill != null)
                {
                    job.JobSkills.Add(new JobSkill
                    {
                        SkillID = skill.SkillID,
                        SkillType = skillDto.SkillType
                    });
                }
            }

            foreach (var roundDto in dto.InterviewRounds)
            {
                job.InterviewRoundTemplates.Add(new JobInterviewRoundTemplate
                {
                    SequenceNo = roundDto.SequenceNo,
                    RoundName = roundDto.RoundName,
                    RoundType = roundDto.RoundType,
                    DurationMinutes = roundDto.DurationMinutes,
                    InterviewMode = roundDto.InterviewMode,
                    IsPanelRound = roundDto.IsPanelRound
                });
            }

            var createdJob = await _jobRepository.AddAsync(job);

            return new JobResponseDto
            (
                createdJob.JobID,
                createdJob.Title,
                createdJob.Description,
                createdJob.MinExperience,
                createdJob.MaxExperience,
                createdJob.CompanyName,
                createdJob.Location,
                createdJob.JobType,
                createdJob.WorkplaceType,
                createdJob.Status,
                createdJob.CreatedAt,
                createdJob.JobSkills.Select(js => new JobSkillDetailDto
                (
                    js.SkillID,
                    js.Skill.SkillName,
                    js.SkillType
                )).ToList(),
                createdJob.InterviewRoundTemplates.Select(r =>
                new JobInterviewRoundTemplateDto(
                r.SequenceNo,
                r.RoundName,
                r.RoundType,
                r.DurationMinutes,
                r.InterviewMode,
                r.IsPanelRound
            )).ToList()
            );
        }

        public async Task<bool> DeleteJobAsync(int jobId)
        {
            var job = await _jobRepository.GetByIdAsync(jobId);
            if (job == null) return false;

            await _jobRepository.DeleteAsync(jobId);
            return true;
        }

        public async Task<List<JobResponseDto>> GetAllJobsAsync()
        {
            var jobs = await _jobRepository.GetAllAsync();
            return jobs.Select(job => new JobResponseDto
            (
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
                job.JobSkills.Select(js => new JobSkillDetailDto
                (
                    js.SkillID,
                    js.Skill.SkillName,
                    js.SkillType
                )).ToList(),
                job.InterviewRoundTemplates.Select(r =>
                new JobInterviewRoundTemplateDto(
                    r.SequenceNo,
                    r.RoundName,
                    r.RoundType,
                    r.DurationMinutes,
                    r.InterviewMode,
                    r.IsPanelRound
                )).ToList()
            )).ToList();
        }

        public async Task<JobResponseDto?> GetJobByIdAsync(int jobId)
        {
            var job = await _jobRepository.GetByIdAsync(jobId);
            if (job == null) return null;

            return new JobResponseDto
            (
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
                job.JobSkills.Select(js => new JobSkillDetailDto
                (
                    js.SkillID,
                    js.Skill.SkillName,
                    js.SkillType
                )).ToList(),
                job.InterviewRoundTemplates.Select(r =>
                new JobInterviewRoundTemplateDto(
                    r.SequenceNo,
                    r.RoundName,
                    r.RoundType,
                    r.DurationMinutes,
                    r.InterviewMode,
                    r.IsPanelRound
                )).ToList()
            );
        }

        public async Task<JobResponseDto?> UpdateJobAsync(int jobId, UpdateJobDto dto)
        {
            var job = await _jobRepository.GetByIdAsync(jobId);
            if (job == null) return null;

            if (!string.IsNullOrEmpty(dto.Title)) job.Title = dto.Title;
            if (!string.IsNullOrEmpty(dto.Description)) job.Description = dto.Description;
            if (dto.MinExperience.HasValue) job.MinExperience = dto.MinExperience.Value;
            if (dto.MaxExperience.HasValue) job.MaxExperience = dto.MaxExperience.Value;
            if (!string.IsNullOrEmpty(dto.CompanyName)) job.CompanyName = dto.CompanyName;
            if (!string.IsNullOrEmpty(dto.Location)) job.Location = dto.Location;
            if (!string.IsNullOrEmpty(dto.JobType)) job.JobType = dto.JobType;
            if (!string.IsNullOrEmpty(dto.WorkplaceType)) job.WorkplaceType = dto.WorkplaceType;
            if (!string.IsNullOrEmpty(dto.Status)) job.Status = dto.Status;
            if (!string.IsNullOrEmpty(dto.ClosedReason)) job.ClosedReason = dto.ClosedReason;

            job.UpdatedAt = DateTime.Now;

            if (dto.Skills != null && dto.Skills.Any())
            {
                job.JobSkills.Clear();
                foreach (var skillDto in dto.Skills)
                {
                    var skill = await _context.Skills.FindAsync(skillDto.SkillID);
                    if (skill != null)
                    {
                        job.JobSkills.Add(new JobSkill
                        {
                            SkillID = skill.SkillID,
                            SkillType = skillDto.SkillType
                        });
                    }
                }
            }

            if (dto.Status?.Equals("Closed", StringComparison.OrdinalIgnoreCase) == true)
            {
                bool hasCandidate = job.SelectedCandidateID.HasValue || dto.SelectedCandidateID.HasValue;
                bool hasReason = !string.IsNullOrWhiteSpace(dto.ClosedReason);

                if (!hasCandidate && !hasReason)
                {
                    throw new InvalidOperationException(
                        "Job cannot be closed without either selecting a candidate or providing a closure reason."
                    );
                }

                if (dto.SelectedCandidateID.HasValue)
                    job.SelectedCandidateID = dto.SelectedCandidateID;
            }

            await _jobRepository.UpdateAsync(job);

            return new JobResponseDto
            (
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
                job.JobSkills.Select(js => new JobSkillDetailDto(
                    js.SkillID, 
                    js.Skill.SkillName, 
                    js.SkillType
                )).ToList(),
                job.InterviewRoundTemplates.Select(r =>
                new JobInterviewRoundTemplateDto(
                    r.SequenceNo,
                    r.RoundName,
                    r.RoundType,
                    r.DurationMinutes,
                    r.InterviewMode,
                    r.IsPanelRound
                )).ToList()
            );
        }
    }

}
