using InsuranceClaims.Enums;
using InsuranceClaims.Helpers;
using InsuranceClaims.Repositories;
using InsuranceClaims.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace InsuranceClaims.Controllers
{
    public class DocumentController : Controller
    {
        private readonly IDocumentService _documentService;
        private readonly ClaimRepository  _claimRepo;
        private readonly SurveyorRepository _surveyorRepo;

        public DocumentController(IDocumentService documentService, ClaimRepository claimRepo,
            SurveyorRepository surveyorRepo)
        {
            _documentService = documentService;
            _claimRepo       = claimRepo;
            _surveyorRepo    = surveyorRepo;
        }

        [HttpGet]
        public async Task<IActionResult> Upload(int id)
        {
            if (!HttpContext.Session.IsLoggedIn()) return RedirectToAction("Login", "Account");
            if (HttpContext.Session.GetUserRole() != "Customer") return RedirectToAction("AccessDenied", "Home");
            if (id <= 0) { TempData["Error"] = "Invalid claim."; return RedirectToAction("MyClaims", "Claim"); }

            var claim = await _claimRepo.GetByIdAsync(id);
            if (claim == null) { TempData["Error"] = $"Claim #{id} not found."; return RedirectToAction("MyClaims", "Claim"); }

            ViewBag.ClaimId    = id;
            ViewBag.ClaimType  = claim.ClaimType;
            ViewBag.PolicyName = claim.Policy?.PolicyName ?? "";
            return View();
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Upload(int claimId, string documentName, IFormFile file)
        {
            if (!HttpContext.Session.IsLoggedIn()) return RedirectToAction("Login", "Account");
            if (claimId <= 0) { TempData["Error"] = "Invalid Claim ID."; return RedirectToAction("MyClaims", "Claim"); }

            var claim = await _claimRepo.GetByIdAsync(claimId);
            if (claim == null) { TempData["Error"] = $"Claim #{claimId} not found."; return RedirectToAction("MyClaims", "Claim"); }
            if (file == null || file.Length == 0) { TempData["Error"] = "Please select a file."; ViewBag.ClaimId = claimId; return View(); }
            if (string.IsNullOrWhiteSpace(documentName)) { TempData["Error"] = "Please select document type."; ViewBag.ClaimId = claimId; return View(); }

            var (success, message) = await _documentService.UploadDocumentAsync(claimId, file, documentName);
            TempData[success ? "Success" : "Error"] = message;
            return RedirectToAction("Details", "Claim", new { id = claimId });
        }

        public async Task<IActionResult> PendingDocuments()
        {
            if (!HttpContext.Session.IsLoggedIn()) return RedirectToAction("Login", "Account");
            var role = HttpContext.Session.GetUserRole();
            if (role != "Officer" && role != "Surveyor") return RedirectToAction("AccessDenied", "Home");

            if (role == "Surveyor")
            {
                var surveyor = await _surveyorRepo.GetByUserIdAsync(HttpContext.Session.GetUserId());
                if (surveyor == null) { TempData["Error"] = "Surveyor profile not found."; return RedirectToAction("Index", "Assessment"); }
                var surveyorDocs = await _documentService.GetPendingDocumentsForSurveyorAsync(surveyor.SurveyorId);
                ViewBag.IsSurveyor = true;
                return View(surveyorDocs);
            }

            var docs = await _documentService.GetPendingDocumentsAsync();
            ViewBag.IsSurveyor = false;
            return View(docs);
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Verify(int documentId, string status)
        {
            if (!HttpContext.Session.IsLoggedIn()) return RedirectToAction("Login", "Account");
            var role = HttpContext.Session.GetUserRole();
            if (role != "Officer" && role != "Surveyor") return RedirectToAction("AccessDenied", "Home");

            if (!Enum.TryParse<VerificationStatus>(status, out var parsedStatus))
            {
                TempData["Error"] = "Invalid status value.";
                return RedirectToAction("PendingDocuments");
            }

            var (success, message) = await _documentService.VerifyDocumentAsync(documentId, parsedStatus);
            TempData[success ? "Success" : "Error"] = message;
            return RedirectToAction("PendingDocuments");
        }
    }
}
