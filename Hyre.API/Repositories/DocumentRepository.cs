using DocumentFormat.OpenXml.Spreadsheet;
using Hyre.API.Data;
using Hyre.API.Interfaces.DocumentVerify;
using Hyre.API.Models;
using Microsoft.EntityFrameworkCore;
using static Hyre.API.Dtos.DocumentVerification.DocumentVerificationDtos;

public class DocumentRepository : IDocumentRepository
{
    private readonly ApplicationDbContext _context;

    public DocumentRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<CandidateDocumentVerification> GetVerificationAsync(string userId, int jobId)
    {
        int candidateId = await GetCandidateIdByUserIdAsync(userId);
        var verification = await _context
            .CandidateDocumentVerifications
            .Include(v => v.Documents)
                .ThenInclude(d => d.DocumentType)
            .FirstOrDefaultAsync(v =>
                v.CandidateId == candidateId &&
                v.JobId == jobId);

        if (verification == null)
            throw new Exception("Verification not found");

        return verification;
    }

    public async Task<List<DocumentType>> GetActiveDocumentTypesAsync()
    {
        return await _context.DocumentTypes
            .Where(x => x.IsActive)
            .OrderBy(x => x.DisplayOrder)
            .ToListAsync();
    }

    private async Task<int> GetCandidateIdByUserIdAsync(string userId)
    {
        var candidate = await _context.Candidates
            .FirstOrDefaultAsync(c => c.UserId == userId);

        if (candidate == null)
            throw new Exception("Candidate profile not found");

        return candidate.CandidateID;
    }

    public async Task<CandidateDocument?> GetCandidateDocumentAsync(int verificationId, int documentTypeId)
    {
        return await _context.CandidateDocuments
            .FirstOrDefaultAsync(x => x.VerificationId == verificationId && x.DocumentTypeId == documentTypeId);
    }

    public async Task AddAsync(CandidateDocument entity)
    {
        _context.CandidateDocuments.Add(entity);
        await _context.SaveChangesAsync();
    }

    public async Task UpdateAsync(CandidateDocument entity)
    {
        _context.CandidateDocuments.Update(entity);
        await _context.SaveChangesAsync();
    }

    public async Task UpdateVerificationAsync(CandidateDocumentVerification entity)
    {
        _context.CandidateDocumentVerifications.Update(entity);
        await _context.SaveChangesAsync();
    }

    public async Task<List<DocumentType>> GetMandatoryDocumentTypesAsync()
    {
        return await _context.DocumentTypes
            .Where(x => x.IsActive && x.IsMandatory)
            .ToListAsync();
    }

}
