using Microsoft.AspNetCore.Mvc;
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

        public RegistrationController()
        {
            _userService = new UserService();
            _dbHelper = new DatabaseHelper();
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
        public IActionResult Learner(LearnerModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
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
