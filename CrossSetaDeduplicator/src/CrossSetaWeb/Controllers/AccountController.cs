using Microsoft.AspNetCore.Mvc;
using CrossSetaLogic.Models;
using CrossSetaLogic.DataAccess;
using System.Threading.Tasks;
using System;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using System.Security.Claims;
using System.Collections.Generic;

namespace CrossSetaWeb.Controllers
{
    public class AccountController : Controller
    {
        private readonly Supabase.Client _supabase;
        private readonly IDatabaseHelper _dbHelper;

        public AccountController(Supabase.Client supabase, IDatabaseHelper dbHelper)
        {
            _supabase = supabase;
            _dbHelper = dbHelper;
        }

        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Register(SignUpViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            try
            {
                await _supabase.InitializeAsync();
                var session = await _supabase.Auth.SignUp(model.Email, model.Password);

                if (session != null && session.User != null)
                {
                    // If Supabase is set to confirm email, the user won't be able to login immediately.
                    // But if auto-confirm is on (dev default), they are logged in.
                    
                    if (session.User.Identities != null && session.User.Identities.Count > 0)
                    {
                        await SignInUser(session.User.Email);
                        TempData["SuccessMessage"] = "Registration successful!";
                        return RedirectToAction("Bulk", "Registration");
                    }
                    else
                    {
                        // Email confirmation required
                        return View("RegistrationConfirmation");
                    }
                }
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", "Registration Failed: " + ex.Message);
            }

            return View(model);
        }

        [HttpGet]
        public IActionResult Login()
        {
            if (User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Bulk", "Registration");
            }
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            try
            {
                await _supabase.InitializeAsync();
                var session = await _supabase.Auth.SignIn(model.Email, model.Password);
                
                if (session != null && session.User != null)
                {
                    await SignInUser(session.User.Email);
                    _dbHelper.LogUserActivity(session.User.Email, "Login", HttpContext.Connection.RemoteIpAddress?.ToString(), "Supabase Login");
                    TempData["SuccessMessage"] = $"Welcome back, {session.User.Email}!";
                    return RedirectToAction("Bulk", "Registration");
                }
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", "Login Failed: " + ex.Message);
            }

            return View(model);
        }

        [HttpGet]
        public IActionResult Callback()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> SetSession(string email)
        {
            // Called by Client-side JS after successful OAuth
            await SignInUser(email);
            _dbHelper.LogUserActivity(email, "Login", HttpContext.Connection.RemoteIpAddress?.ToString(), "OAuth Callback");
            TempData["SuccessMessage"] = $"Welcome back, {email}!";
            return RedirectToAction("Index", "Home");
        }
        
        [HttpGet]
        public async Task<IActionResult> Logout()
        {
            if (User.Identity.IsAuthenticated)
            {
                _dbHelper.LogUserActivity(User.Identity.Name, "Logout", HttpContext.Connection.RemoteIpAddress?.ToString(), "User Logout");
            }
            await _supabase.InitializeAsync();
            await _supabase.Auth.SignOut();
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Login");
        }

        private async Task SignInUser(string email)
        {
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, email),
                new Claim(ClaimTypes.Email, email)
            };

            var claimsIdentity = new ClaimsIdentity(
                claims, CookieAuthenticationDefaults.AuthenticationScheme);

            var authProperties = new AuthenticationProperties
            {
                IsPersistent = true,
                ExpiresUtc = DateTime.UtcNow.AddHours(8)
            };

            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme, 
                new ClaimsPrincipal(claimsIdentity), 
                authProperties);
        }
    }
}
