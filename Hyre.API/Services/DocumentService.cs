using Hyre.API.Data;
using Hyre.API.Interfaces.DocumentVerify;
using Hyre.API.Models;
using Microsoft.EntityFrameworkCore;
using static Hyre.API.Dtos.DocumentVerification.DocumentVerificationDtos;

namespace Hyre.API.Services
{
    public class DocumentService : IDocumentService
    {
        private readonly IDocumentRepository _repository;
        private readonly ApplicationDbContext _context;

        public DocumentService(IDocumentRepository repository, ApplicationDbContext context)
        {
            _repository = repository;
            _context = context;
        }

        public async Task<List<RequiredDocumentDto>> GetRequiredDocumentsAsync(string userId, int jobId)
        {
            

            var verification = await _repository.GetVerificationAsync(userId, jobId);

            var documentTypes = await _repository.GetActiveDocumentTypesAsync();

            var result = new List<RequiredDocumentDto>();

            foreach (var docType in documentTypes)
            {
                var candidateDoc = verification.Documents
                    .FirstOrDefault(d =>
                        d.DocumentTypeId == docType.DocumentTypeId);

                result.Add(new RequiredDocumentDto(
                    docType.DocumentTypeId,
                    docType.Name,
                    docType.IsMandatory,
                    docType.AllowedFormats,
                    docType.DisplayOrder,
                    candidateDoc?.Status ?? "NotUploaded",
                    candidateDoc?.FilePath
                ));
            }

            return result;
        }

        public async Task UploadDocumentAsync(string userId, UploadDocumentDto dto)
        {
            var verification = await _repository.GetVerificationAsync(userId, dto.JobId);

            if (verification.Status != "ActionRequired" &&
                verification.Status != "ReuploadRequired")
                throw new Exception("Upload not allowed in current state");

            if (dto.File == null || dto.File.Length == 0)
                throw new Exception("Invalid file");

            string folder = Path.Combine(Directory.GetCurrentDirectory(), "PrivateFiles", "Uploads", $"Candidate_{verification.CandidateId}_{dto.JobId}");

            Directory.CreateDirectory(folder);

            string fileName = $"{Guid.NewGuid()}_{dto.File.FileName}";
            string filePath = Path.Combine(folder, fileName);

            using var stream = new FileStream(filePath, FileMode.Create);
            await dto.File.CopyToAsync(stream);

            var existing = await _repository
                .GetCandidateDocumentAsync(verification.VerificationId, dto.DocumentTypeId);

            if (existing == null)
            {
                var entity = new CandidateDocument
                {
                    VerificationId = verification.VerificationId,
                    DocumentTypeId = dto.DocumentTypeId,
                    FilePath = filePath,
                    Status = "Uploaded",
                    UploadedAt = DateTime.UtcNow,
                    UploadedBy = verification.CandidateId
                };

                await _repository.AddAsync(entity);
            }
            else
            {
                existing.FilePath = filePath;
                existing.Status = "Uploaded";
                existing.UploadedAt = DateTime.UtcNow;
                existing.Version += 1;

                await _repository.UpdateAsync(existing);
            }
        }

        public async Task SubmitForVerificationAsync(string userId, SubmitForVerificationDto dto)
        {
            var verification = await _repository.GetVerificationAsync(userId, dto.JobId);

            if (verification.Status != "ActionRequired")
                throw new Exception("Verification already submitted");

            var mandatoryDocs = await _repository.GetMandatoryDocumentTypesAsync();

            foreach (var doc in mandatoryDocs)
            {
                var uploaded = verification.Documents
                    .Any(d => d.DocumentTypeId == doc.DocumentTypeId
                           && d.Status == "Uploaded");

                if (!uploaded)
                    throw new Exception($"Mandatory document missing: {doc.Name}");
            }

            verification.Status = "UnderVerification";
            await _repository.UpdateVerificationAsync(verification);
        }

        public async Task<List<DocumentJobDto>> GetJobsWithPendingVerificationsAsync()
        {
            var jobs = await _repository.GetJobsAsync();

            var result = new List<DocumentJobDto>();

            foreach (var job in jobs)
            {
                var pendingCount = await _context.CandidateDocumentVerifications
                    .Where(v => v.JobId == job.JobID && v.Status == "UnderVerification")
                    .CountAsync();

                result.Add(new DocumentJobDto(
                    job.JobID,
                    job.Title,
                    job.Description ?? string.Empty,
                    job.CompanyName,
                    job.Location ?? string.Empty,
                    job.JobType ?? string.Empty,
                    job.WorkplaceType ?? string.Empty,
                    job.Status,
                    job.MinExperience,
                    job.MaxExperience,
                    job.CreatedAt,
                    pendingCount
                ));
            }

            return result;
        }

        public async Task<List<CandidateDetailDto>> GetCandidatesByVerificationStatusAsync(int jobId, string status)
        {
            var candidateVerifications = await _context.CandidateDocumentVerifications
                .Where(v => v.JobId == jobId && v.Status == status)
                .Include(v => v.Candidate)
                    .ThenInclude(c => c.CandidateSkills)
                    .ThenInclude(cs => cs.Skill)
                .ToListAsync();

            var result = new List<CandidateDetailDto>();

            foreach (var verification in candidateVerifications)
            {
                var candidate = verification.Candidate;
                var candidateSkills = candidate.CandidateSkills?
                    .Where(cs => cs.Skill != null)
                    .Select(cs => new CandidateSkillDto(
                        cs.SkillID,
                        cs.Skill.SkillName,
                        cs.YearsOfExperience
                    ))
                    .ToList() ?? new List<CandidateSkillDto>();

                result.Add(new CandidateDetailDto(
                    candidate.CandidateID,
                    verification.VerificationId,
                    candidate.FirstName,
                    candidate.LastName,
                    candidate.Email,
                    candidate.Phone,
                    candidate.ExperienceYears,
                    candidate.ResumePath,
                    candidate.Status,
                    candidateSkills
                ));
            }

            return result;
        }

        public async Task<HrVerificationDetailDto> GetVerificationForHrAsync(int verificationId)
        {
            var verification = await _repository.GetVerificationForHrAsync(verificationId);

            var docs = verification.Documents.Select(d =>
                new HrDocumentDto(
                    d.DocumentId,
                    d.DocumentTypeId,
                    d.DocumentType.Name,
                    d.Status,
                    d.FilePath,
                    d.UploadedAt,
                    d.UploadedBy,
                    d.VerifiedAt,
                    d.VerifiedBy,
                    d.HrComment
                )).ToList();

            return new HrVerificationDetailDto(
                verification.VerificationId,
                verification.Status,
                docs
            );
        }
        public async Task ProcessHrActionAsync(string hrUserId, HrVerificationActionDto dto)
        {
            var verification = await _repository
                .GetVerificationWithDocumentsAsync(dto.VerificationId);

            if (verification.Status != "UnderVerification")
                throw new Exception("Verification not in review state");

            if (dto.Action == "Reject")
            {
                verification.Status = "Completed";
                verification.FinalDecision = "Rejected";
                verification.CompletedAt = DateTime.UtcNow;
                verification.HrComment = dto.Comment;

                await _repository.UpdateVerificationAsync(verification);
                return;
            }

            if (dto.Documents == null || !dto.Documents.Any())
                throw new Exception("Document decisions required");

            bool anyReupload = false;

            foreach (var docAction in dto.Documents)
            {
                var doc = verification.Documents
                    .FirstOrDefault(x => x.DocumentTypeId == docAction.DocumentTypeId);

                if (doc == null)
                    throw new Exception($"Document not found: {docAction.DocumentTypeId}");

                doc.Status = docAction.Status;
                doc.VerifiedAt = DateTime.UtcNow;
                doc.VerifiedBy = hrUserId;
                doc.HrComment = dto.Comment;

                if (docAction.Status == "ReuploadRequired")
                    anyReupload = true;

                await _repository.UpdateCandidateDocumentAsync(doc);
            }

            if (dto.Action == "Approve" && anyReupload)
                throw new Exception("Cannot approve when re-upload is required");

            if (anyReupload)
            {
                verification.Status = "ReuploadRequired";
            }
            else
            {
                verification.Status = "Completed";
                verification.FinalDecision = "Accepted";
                verification.CompletedAt = DateTime.UtcNow;
            }

            await _repository.UpdateVerificationAsync(verification);
        }
        public async Task<(byte[] fileBytes, string fileName)> GetDocumentForHrAsync(int documentId)
        {
            var doc = await _repository.GetDocumentByIdAsync(documentId);

            if (!System.IO.File.Exists(doc.FilePath))
                throw new Exception("File not found");

            var bytes = await File.ReadAllBytesAsync(doc.FilePath);
            var fileName = Path.GetFileName(doc.FilePath);

            return (bytes, fileName);
        }
        public async Task<CandidateVerificationDetailDto> GetCandidateVerificationDetailAsync(string userId, int jobId)
        {
            var verification = await _repository
                .GetVerificationForCandidateAsync(userId, jobId);

            var docs = verification.Documents.Select(d =>
                new CandidateDocumentDto(
                    d.DocumentId,
                    d.DocumentTypeId,
                    d.DocumentType.Name,
                    d.Status,
                    d.FilePath,
                    d.UploadedAt
                )).ToList();

            return new CandidateVerificationDetailDto(
                verification.VerificationId,
                verification.Status,
                docs
            );
        }
        public async Task<List<CandidateJobDto>> GetJobsWithPendingDocumentSubmissionAsync(string userId)
        {
            var jobs = await _repository.GetJobsWithPendingDocumentSubmissionAsync(userId);

            return jobs.Select(job => new CandidateJobDto(
                job.JobID,
                job.Title,
                job.Description ?? string.Empty,
                job.CompanyName,
                job.Location ?? string.Empty,
                job.JobType ?? string.Empty,
                job.WorkplaceType ?? string.Empty,
                job.Status,
                job.MinExperience,
                job.MaxExperience
            )).ToList();
        }

    }
}
