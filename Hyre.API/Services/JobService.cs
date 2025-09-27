using Hyre.API.Data;
using Hyre.API.Dtos;
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

        public async Task<JobResponseDto> CreateJobAsync(CreateJobDto dto, int createdByUserId)
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
                )).ToList()
            );
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
                )).ToList()
            );
        }
    }

}
