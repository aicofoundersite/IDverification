using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Microsoft.Data.SqlClient;
using CrossSetaWeb.Models;
using CrossSetaWeb.Services;
using CrossSetaWeb.DataAccess;
using Microsoft.AspNetCore.Authorization;

namespace CrossSetaWeb.Controllers
{
    public class RegistrationController : Controller
    {
        private readonly IUserService _userService;
        private readonly IDatabaseHelper _dbHelper;
        private readonly IKYCService _kycService;
        private readonly IBulkRegistrationService _bulkService;
        private readonly IDatabaseValidationService _validationService;
        private readonly IHomeAffairsImportService _importService;

        private readonly IValidationProgressService _progressService;
        private readonly RegistrationController _controller; // Self-ref not needed, just for context

        public RegistrationController(
            IUserService userService, 
            IDatabaseHelper dbHelper,
            IKYCService kycService,
            IBulkRegistrationService bulkService,
            IDatabaseValidationService validationService,
            IHomeAffairsImportService importService,
            IValidationProgressService progressService)
        {
            _userService = userService;
            _dbHelper = dbHelper;
            _kycService = kycService;
            _bulkService = bulkService;
            _validationService = validationService;
            _importService = importService;
            _progressService = progressService;
        }

        [HttpGet]
        [Authorize]
        public IActionResult Bulk()
        {
            if (HttpContext.User.Identity.IsAuthenticated && HttpContext.User.Identity.Name.Equals("koosms02@gmail.com", StringComparison.OrdinalIgnoreCase))
            {
                ViewBag.IsSuperUser = true;
                ViewBag.UserActivityLogs = _dbHelper.GetUserActivityLogs();
            }
            return View();
        }

        [HttpPost]
        [Authorize]
        public IActionResult Bulk(IFormFile csvFile)
        {
            if (csvFile == null || csvFile.Length == 0)
            {
                ModelState.AddModelError("", "Please upload a valid CSV file.");
                return View();
            }

            if (!csvFile.FileName.EndsWith(".csv", StringComparison.OrdinalIgnoreCase))
            {
                ModelState.AddModelError("", "Only CSV files are allowed.");
                return View();
            }

            try
            {
                var result = _bulkService.ProcessBulkFile(csvFile);

                if (result.SuccessCount > 0)
                {
                    TempData["SuccessMessage"] = $"Bulk Import Complete! {result.SuccessCount} learners registered successfully.";
                }

                if (result.FailureCount > 0)
                {
                    // Generate Error Report
                    string reportFileName = $"BulkErrors_{DateTime.Now:yyyyMMddHHmmss}_{Guid.NewGuid()}.csv";
                    string reportsDir = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "reports");
                    if (!Directory.Exists(reportsDir)) Directory.CreateDirectory(reportsDir);
                    
                    string reportPath = Path.Combine(reportsDir, reportFileName);
                    
                    using (var sw = new StreamWriter(reportPath))
                    {
                        sw.WriteLine("RowNumber,NationalID,ErrorMessage");
                        foreach (var error in result.ErrorDetails)
                        {
                            sw.WriteLine($"{error.RowNumber},{error.NationalID},\"{error.Message.Replace("\"", "\"\"")}\"");
                        }
                    }

                    TempData["ErrorMessage"] = $"{result.FailureCount} records failed. Please download the error report for details.";
                    TempData["ReportFileName"] = reportFileName;
                    
                    // Show top 5 errors in UI
                    foreach (var error in result.ErrorDetails.Take(5))
                    {
                         ModelState.AddModelError("", $"Row {error.RowNumber}: {error.Message}");
                    }
                    if (result.FailureCount > 5)
                    {
                        ModelState.AddModelError("", $"... and {result.FailureCount - 5} more errors.");
                    }
                }
                else if (result.SuccessCount == 0)
                {
                    ModelState.AddModelError("", "No records were successfully imported.");
                }

                return View();
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", "Fatal error during import: " + ex.Message);
                return View();
            }
        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> UploadHomeAffairs(IFormFile file)
        {
            if (file == null || file.Length == 0)
            {
                TempData["ErrorMessage"] = "Please select a valid CSV file.";
                return RedirectToAction("Bulk");
            }

            try
            {
                using (var stream = new StreamReader(file.OpenReadStream()))
                {
                    string content = await stream.ReadToEndAsync();
                    var result = _importService.ImportFromContent(content);
                    
                    if (result.Success)
                    {
                        TempData["SuccessMessage"] = $"Reference Data Updated Successfully! ({result.RecordsProcessed} records).";
                        if (result.Errors != null && result.Errors.Count > 0)
                        {
                            TempData["ErrorMessage"] = "Some rows failed: " + result.Errors[0];
                        }
                    }
                    else
                    {
                        TempData["ErrorMessage"] = "Import Failed: " + (result.Errors?.FirstOrDefault() ?? "Unknown error");
                    }
                }
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Upload Error: " + ex.Message;
            }

            return RedirectToAction("Bulk");
        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> ValidateDatabase()
        {
            // Start background job
            string jobId = Guid.NewGuid().ToString();
            _progressService.StartValidation(jobId);

            // Run in background (Fire and Forget)
            _ = Task.Run(async () => 
            {
                try
                {
                    // 1. Refresh Home Affairs Data (Skipped for speed - User Request)
                    // string googleSheetUrl = "https://docs.google.com/spreadsheets/d/1eQjxSsuOuXU20xG0gGgmR0Agn7WvudJd/export?format=csv&gid=1067188886";
                    // _progressService.UpdateProgress(jobId, 0, 0, "Updating Reference Data...");
                    
                    // var importResult = await _importService.ImportFromUrlAsync(googleSheetUrl);
                    // string importMsg = importResult.Success ? $"Source Updated ({importResult.RecordsProcessed} records)." : "Source Update Failed.";

                    // 2. Run Validation
                    _progressService.UpdateProgress(jobId, 0, 0, "Starting Validation...");
                    var result = _validationService.ValidateDatabase(jobId);

                    // Generate Report
                    string reportFileName = $"ValidationReport_{DateTime.Now:yyyyMMddHHmmss}_{Guid.NewGuid()}.csv";
                    string reportsDir = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "reports");
                    if (!Directory.Exists(reportsDir)) Directory.CreateDirectory(reportsDir);
                    
                    string reportPath = Path.Combine(reportsDir, reportFileName);

                    // Write CSV
                    using (var sw = new StreamWriter(reportPath))
                    {
                        await sw.WriteLineAsync("NationalID,FirstName,LastName,Status,Message");
                        foreach (var d in result.Details)
                        {
                            var msg = d.Message?.Replace("\"", "\"\"") ?? "";
                            await sw.WriteLineAsync($"{d.NationalID},{d.FirstName},{d.LastName},{d.Status},\"{msg}\"");
                        }
                    }
                    
                    result.ReportFileName = reportFileName;
                    _progressService.CompleteValidation(jobId, result);
                }
                catch (Exception ex)
                {
                     // Log error
                     Console.WriteLine($"Background Job Error: {ex.Message}");
                     _progressService.UpdateProgress(jobId, 0, 0, $"Error: {ex.Message}");
                }
            });

            return Json(new { jobId = jobId });
        }

        [HttpGet]
        [Authorize]
        public IActionResult GetValidationStatus(string jobId)
        {
            var progress = _progressService.GetProgress(jobId);
            if (progress == null) return NotFound();
            return Json(progress);
        }
        
        [HttpGet]
        [Authorize]
        public IActionResult DownloadReport(string fileName)
        {
            if (string.IsNullOrEmpty(fileName)) return BadRequest("Filename is missing");

            string path = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "reports", fileName);
            if (!System.IO.File.Exists(path)) return NotFound();

            var bytes = System.IO.File.ReadAllBytes(path);
            return File(bytes, "text/csv", fileName);
        }

        [HttpGet]
        public IActionResult User()
        {
            return View();
        }

        [HttpPost]
        public IActionResult User(RegisterUserViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            try
            {
                _userService.RegisterUser(model.ToUserModel(), model.Password);
                TempData["SuccessMessage"] = "User Registered Successfully!";
                return RedirectToAction("User");
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", "Error registering user: " + ex.Message);
                return View(model);
            }
        }

        [HttpGet]
        public IActionResult Learner()
        {
            return View(new LearnerModel());
        }

        [HttpPost]
        public async Task<IActionResult> Learner(LearnerModel model, IFormFile kycDocument)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            // KYC Document Verification
            if (kycDocument != null && kycDocument.Length > 0)
            {
                var kycResult = await _kycService.VerifyDocumentAsync(kycDocument, model.NationalID);
                if (!kycResult.IsSuccess)
                {
                    ModelState.AddModelError("KYC", $"KYC Verification Failed: {kycResult.ErrorMessage}");
                    return View(model);
                }
                
                // If KYC is successful, we mark the learner as verified
                model.IsVerified = true;
            }

            try
            {
                // Basic server-side validation for age calc if needed, 
                // but we trust the model binding for now.
                
                _dbHelper.InsertLearner(model);
                TempData["SuccessMessage"] = "Learner Registered Successfully!";
                return RedirectToAction("Learner");
            }
            catch (SqlException ex) when (ex.Number == 51000) // Custom error from SP
            {
                 ModelState.AddModelError("NationalID", "This National ID is already registered.");
                 return View(model);
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", "Error registering learner: " + ex.Message);
                return View(model);
            }
        }
    }
}
