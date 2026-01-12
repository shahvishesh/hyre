using Hyre.API.Interfaces.DocumentVerify;
using Hyre.API.Models;
using static Hyre.API.Dtos.DocumentVerification.DocumentVerificationDtos;

namespace Hyre.API.Services
{
    public class DocumentService : IDocumentService
    {
        private readonly IDocumentRepository _repository;

        public DocumentService(IDocumentRepository repository)
        {
            _repository = repository;
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



    }
}
