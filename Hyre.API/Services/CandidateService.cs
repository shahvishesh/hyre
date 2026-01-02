using ClosedXML.Excel;
using Hyre.API.Data;
using Hyre.API.Dtos.Candidate;
using Hyre.API.Interfaces.Candidates;
using Hyre.API.Models;
using Hyre.API.Repositories;
using Microsoft.EntityFrameworkCore;

namespace Hyre.API.Services
{
    public class CandidateService : ICandidateService
    {
        private readonly ICandidateRepository _repository;
        private readonly ApplicationDbContext _context;

        public CandidateService(ICandidateRepository repository, ApplicationDbContext context)
        {
            _repository = repository;
            _context = context;
        }

        // Manual creation
        public async Task<CandidateDto> CreateCandidateAsync(CreateCandidateDto dto, string createdByUserId, IFormFile? resumeFile = null)
        {
            var candidate = new Candidate
            {
                FirstName = dto.FirstName,
                LastName = dto.LastName,
                Email = dto.Email,
                Phone = dto.Phone,
                ExperienceYears = dto.ExperienceYears,
                CreatedBy = createdByUserId,
                CandidateSkills = new List<CandidateSkill>()
            };

            if (dto.Skills != null)
            {
                foreach (var skillDto in dto.Skills)
                {
                    var skill = await _context.Skills.FindAsync(skillDto.SkillID);
                    if (skill != null)
                    {
                        candidate.CandidateSkills.Add(new CandidateSkill
                        {
                            SkillID = skill.SkillID,
                            YearsOfExperience = skillDto.YearsOfExperience,
                            AddedBy = createdByUserId
                        });
                    }
                }
            }

            await _repository.AddCandidateAsync(candidate);

            if (resumeFile != null)
            {
                ValidateResumeFile(resumeFile);
                var resumePath = await SaveResumeFileAsync(candidate.CandidateID, resumeFile);
                await _repository.UpdateResumePathAsync(candidate.CandidateID, resumePath);
                candidate.ResumePath = resumePath;
            }

            var candidateSkills = candidate.CandidateSkills.Select(cs =>
                new CandidateSkillDto(cs.SkillID, cs.Skill.SkillName, cs.YearsOfExperience)).ToList();

            return new CandidateDto(
                candidate.CandidateID,
                candidate.FirstName,
                candidate.LastName,
                candidate.Email,
                candidate.Phone,
                candidate.ExperienceYears,
                candidate.ResumePath,
                candidate.Status,
                candidateSkills
            );
        }

        //Excel workbook import 
        public async Task ImportFromWorkbookAsync(IFormFile file, string createdByUserId)
        {
            using var stream = new MemoryStream();
            await file.CopyToAsync(stream);
            using var workbook = new XLWorkbook(stream);

            var candidateSheet = workbook.Worksheet("Candidates");
            var skillSheet = workbook.Worksheet("Skills");
            var mappingSheet = workbook.Worksheet("CandidateSkills");

            var skillMap = await ImportSkillsAsync(skillSheet);
            var candidateMap = await ImportCandidatesAsync(candidateSheet, createdByUserId);
            await ImportCandidateSkillsAsync(mappingSheet, candidateMap, skillMap, createdByUserId);
        }

        private async Task<Dictionary<string, int>> ImportSkillsAsync(IXLWorksheet sheet)
        {
            var map = new Dictionary<string, int>();
            foreach (var row in sheet.RowsUsed().Skip(1))
            {
                var code = row.Cell(1).GetString();
                var name = row.Cell(2).GetString();
                if (string.IsNullOrWhiteSpace(name)) continue;

                var skill = await _context.Skills.FirstOrDefaultAsync(s => s.SkillName == name)
                            ?? new Skill { SkillName = name };

                _context.Skills.Update(skill);
                await _context.SaveChangesAsync();
                map[code] = skill.SkillID;
            }
            return map;
        }

        public async Task<List<CandidateDto>> GetAllCandidatesAsync()
        {
            var candidates = await _repository.GetAllCandidatesAsync();

            return candidates.Select(c => new CandidateDto(
                c.CandidateID,
                c.FirstName,
                c.LastName,
                c.Email,
                c.Phone,
                c.ExperienceYears,
                c.ResumePath,
                c.Status,
                c.CandidateSkills.Select(cs => new CandidateSkillDto(
                    cs.SkillID,
                    cs.Skill.SkillName,
                    cs.YearsOfExperience
                )).ToList()
            )).ToList();
        }

        // Add this method to the CandidateService class
        public async Task<CandidateDto?> GetCandidateByIdAsync(int candidateId)
        {
            var candidate = await _repository.GetCandidateByIdAsync(candidateId);
            if (candidate == null) return null;

            var candidateSkills = candidate.CandidateSkills.Select(cs =>
                new CandidateSkillDto(cs.SkillID, cs.Skill.SkillName, cs.YearsOfExperience)).ToList();

            return new CandidateDto(
                candidate.CandidateID,
                candidate.FirstName,
                candidate.LastName,
                candidate.Email,
                candidate.Phone,
                candidate.ExperienceYears,
                candidate.ResumePath,
                candidate.Status,
                candidateSkills
            );
        }

        private async Task<Dictionary<string, int>> ImportCandidatesAsync(IXLWorksheet sheet, string createdBy)
        {
            var map = new Dictionary<string, int>();
            foreach (var row in sheet.RowsUsed().Skip(1))
            {
                var code = row.Cell(1).GetString();
                var firstName = row.Cell(2).GetString();
                var lastName = row.Cell(3).GetString();
                var email = row.Cell(4).GetString();
                var phone = row.Cell(5).GetString();
                var expText = row.Cell(6).GetString();

                var candidate = new Candidate
                {
                    FirstName = firstName,
                    LastName = lastName,
                    Email = email,
                    Phone = phone,
                    ExperienceYears = decimal.TryParse(expText, out var exp) ? exp : null,
                    CreatedBy = createdBy
                };
                _context.Candidates.Add(candidate);
                await _context.SaveChangesAsync();
                map[code] = candidate.CandidateID;
            }
            return map;
        }

        private async Task ImportCandidateSkillsAsync(IXLWorksheet sheet, Dictionary<string, int> candidateMap, Dictionary<string, int> skillMap, string createdBy)
        {
            foreach (var row in sheet.RowsUsed().Skip(1))
            {
                var candidateCode = row.Cell(1).GetString();
                var skillCode = row.Cell(2).GetString();
                var expText = row.Cell(3).GetString();

                if (!candidateMap.ContainsKey(candidateCode) || !skillMap.ContainsKey(skillCode))
                    continue;

                _context.CandidateSkills.Add(new CandidateSkill
                {
                    CandidateID = candidateMap[candidateCode],
                    SkillID = skillMap[skillCode],
                    YearsOfExperience = decimal.TryParse(expText, out var yrs) ? yrs : null,
                    AddedBy = createdBy
                });
            }
            await _context.SaveChangesAsync();
        }

        private void ValidateResumeFile(IFormFile resumeFile)
        {
            var allowedExtensions = new[] { ".pdf" };
            var ext = Path.GetExtension(resumeFile.FileName).ToLowerInvariant();
            if (!allowedExtensions.Contains(ext))
                throw new InvalidOperationException("Invalid file type. Only PDF allowed.");

            const long maxFileSize = 5 * 1024 * 1024;
            if (resumeFile.Length > maxFileSize)
                throw new InvalidOperationException("File too large. Max 5 MB allowed.");
        }

        private async Task<string> SaveResumeFileAsync(int candidateId, IFormFile resumeFile)
        {
            var folder = Path.Combine(Directory.GetCurrentDirectory(), "PrivateFiles", "Resumes", $"Candidate_{candidateId}");
            Directory.CreateDirectory(folder);

            var fileName = $"{Guid.NewGuid()}_{Path.GetFileName(resumeFile.FileName).Replace(" ", "_")}";
            var path = Path.Combine(folder, fileName);
            using var stream = new FileStream(path, FileMode.Create);
            await resumeFile.CopyToAsync(stream);

            return Path.Combine("Resumes", $"Candidate_{candidateId}", fileName).Replace("\\", "/");
        }

        public async Task<bool> CandidateExistsAsync(int candidateId)
        {
            var candidate = await _repository.GetCandidateByIdAsync(candidateId);
            return candidate != null;
        }

        public async Task<(byte[] fileBytes, string fileName, string contentType)?> GetCandidateResumeAsync(int candidateId)
        {
            var candidate = await _repository.GetCandidateByIdAsync(candidateId);
            if (candidate == null || string.IsNullOrEmpty(candidate.ResumePath))
                return null;

            var resumeFullPath = Path.Combine(Directory.GetCurrentDirectory(), "PrivateFiles", candidate.ResumePath);

            if (!File.Exists(resumeFullPath))
                return null;

            var fileBytes = await File.ReadAllBytesAsync(resumeFullPath);
            var fileName = Path.GetFileName(resumeFullPath);
            var contentType = "application/pdf"; 

            return (fileBytes, fileName, contentType);
        }
    }

}
