using InsuranceClaims.Enums;
using InsuranceClaims.Models.Entities;
using InsuranceClaims.Repositories;
using InsuranceClaims.Services.Interfaces;

namespace InsuranceClaims.Services.Implementations
{
    public class DocumentService : IDocumentService
    {
        private readonly DocumentRepository  _docRepo;
        private readonly ClaimRepository     _claimRepo;
        private readonly IWebHostEnvironment _env;

        public DocumentService(DocumentRepository docRepo, ClaimRepository claimRepo, IWebHostEnvironment env)
        {
            _docRepo   = docRepo;
            _claimRepo = claimRepo;
            _env       = env;
        }

        public async Task<List<ClaimDocument>> GetPendingDocumentsAsync()
            => await _docRepo.GetPendingAsync();

        public async Task<List<ClaimDocument>> GetPendingDocumentsForSurveyorAsync(int surveyorId)
            => await _docRepo.GetPendingForSurveyorAsync(surveyorId);

        public async Task<List<ClaimDocument>> GetDocumentsByClaimAsync(int claimId)
            => await _docRepo.GetByClaimAsync(claimId);

        public async Task<(bool Success, string Message)> UploadDocumentAsync(
            int claimId, IFormFile file, string documentName)
        {
            if (file == null || file.Length == 0)
                return (false, "Please select a valid file.");

            var claim = await _claimRepo.GetByIdAsync(claimId);
            if (claim == null)
                return (false, $"Claim #{claimId} not found.");

            // ── Save file to wwwroot/uploads/ ────────────────────────────────
            var uploadsDir = Path.Combine(_env.WebRootPath, "uploads");
            if (!Directory.Exists(uploadsDir)) Directory.CreateDirectory(uploadsDir);

            var ext      = Path.GetExtension(file.FileName);
            var fileName = $"{Guid.NewGuid()}{ext}";
            var fullPath = Path.Combine(uploadsDir, fileName);

            await using (var stream = new FileStream(fullPath, FileMode.Create))
                await file.CopyToAsync(stream);

            // ── FIX: If a REJECTED doc of the same type exists, REPLACE it ──
            // This prevents count from increasing on re-upload
            var existingRejected = claim.Documents
                .FirstOrDefault(d => d.DocumentName == documentName
                               && d.VerificationStatus == VerificationStatus.REJECTED);

            if (existingRejected != null)
            {
                // Replace: update the existing record back to PENDING with new file
                existingRejected.FilePath           = $"/uploads/{fileName}";
                existingRejected.VerificationStatus = VerificationStatus.PENDING;
                await _docRepo.UpdateAsync(existingRejected);

                // Log tracking
                await _claimRepo.AddTrackingAsync(new ClaimTracking
                {
                    ClaimId   = claimId,
                    Status    = "DOCUMENT_REUPLOADED",
                    Remarks   = $"Customer re-uploaded '{documentName}'. Marked PENDING for re-verification.",
                    UpdatedAt = DateTime.Now
                });

                return (true, $"'{documentName}' re-uploaded successfully and is pending re-verification.");
            }

            // ── Normal upload: add new record ────────────────────────────────
            var doc = new ClaimDocument
            {
                ClaimId            = claimId,
                DocumentName       = documentName,
                FilePath           = $"/uploads/{fileName}",
                VerificationStatus = VerificationStatus.PENDING
            };
            await _docRepo.AddAsync(doc);

            // Move claim REGISTERED → UNDER_REVIEW
            if (claim.ClaimStatus == ClaimStatus.REGISTERED)
            {
                claim.ClaimStatus = ClaimStatus.UNDER_REVIEW;
                await _claimRepo.UpdateAsync(claim);
                await _claimRepo.AddTrackingAsync(new ClaimTracking
                {
                    ClaimId   = claimId,
                    Status    = "UNDER_REVIEW",
                    Remarks   = "Document uploaded. Claim moved to Under Review.",
                    UpdatedAt = DateTime.Now
                });
            }

            return (true, $"'{documentName}' uploaded successfully.");
        }

        public async Task<(bool Success, string Message)> VerifyDocumentAsync(
            int documentId, VerificationStatus status)
        {
            var doc = await _docRepo.GetByIdAsync(documentId);
            if (doc == null) return (false, "Document not found.");

            doc.VerificationStatus = status;
            await _docRepo.UpdateAsync(doc);

            // Check if ALL non-rejected docs for this claim are now verified
            var allDocs      = await _docRepo.GetByClaimAsync(doc.ClaimId);
            var activeDocs   = allDocs.Where(d => d.VerificationStatus != VerificationStatus.REJECTED).ToList();
            bool allVerified = activeDocs.Any() && activeDocs.All(d => d.VerificationStatus == VerificationStatus.VERIFIED);

            if (allVerified)
            {
                await _claimRepo.AddTrackingAsync(new ClaimTracking
                {
                    ClaimId   = doc.ClaimId,
                    Status    = "DOCS_VERIFIED",
                    Remarks   = "All documents verified by surveyor. Ready for assessment.",
                    UpdatedAt = DateTime.Now
                });
            }

            return (true, $"Document marked as {status}.");
        }
    }
}
