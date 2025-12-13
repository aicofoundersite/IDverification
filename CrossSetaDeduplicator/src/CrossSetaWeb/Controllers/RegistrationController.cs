using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Microsoft.Data.SqlClient;
using CrossSetaWeb.Models;
using CrossSetaWeb.Services;
using CrossSetaWeb.DataAccess;

namespace CrossSetaWeb.Controllers
{
    public class RegistrationController : Controller
    {
        private readonly UserService _userService;
        private readonly DatabaseHelper _dbHelper;
        private readonly KYCService _kycService;

        public RegistrationController(KYCService kycService)
        {
            _userService = new UserService();
            _dbHelper = new DatabaseHelper();
            _kycService = kycService;
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
