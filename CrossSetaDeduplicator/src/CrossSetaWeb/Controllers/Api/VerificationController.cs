using Microsoft.AspNetCore.Mvc;
using CrossSetaWeb.DataAccess;
using CrossSetaWeb.Models;
using Microsoft.Extensions.Logging;
using System;

namespace CrossSetaWeb.Controllers.Api
{
    [Route("api/[controller]")]
    [ApiController]
    public class VerificationController : ControllerBase
    {
        private readonly DatabaseHelper _dbHelper;
        private readonly ILogger<VerificationController> _logger;

        public VerificationController(ILogger<VerificationController> logger)
        {
            _dbHelper = new DatabaseHelper();
            _logger = logger;
        }

        [HttpGet("{id}")]
        public IActionResult Verify(string id)
        {
            if (string.IsNullOrEmpty(id))
                return BadRequest("ID is required");

            var learner = _dbHelper.GetLearnerByNationalID(id);
            if (learner != null)
            {
                return Ok(new { 
                    exists = true, 
                    learner = new { 
                        learner.NationalID, 
                        learner.FirstName, 
                        learner.LastName, 
                        learner.Role,
                        learner.SetaName,
                        learner.IsVerified
                    }
                });
            }
            
            return Ok(new { exists = false });
        }

        [HttpGet("home-affairs/{id}")]
        public IActionResult VerifyHomeAffairs(string id, [FromQuery] string surname = null)
        {
            if (string.IsNullOrEmpty(id)) return BadRequest("ID is required");

            _logger.LogInformation("Verification Request - ID: {ID}, Surname: {Surname}", id, surname);

            var citizen = _dbHelper.GetHomeAffairsCitizen(id);
            if (citizen != null)
            {
                string status = "Alive";
                string verificationResult = "VERIFIED"; // Default if no surname provided

                if (citizen.IsDeceased)
                {
                    status = "Deceased";
                    verificationResult = "DECEASED";
                }
                else if (!string.IsNullOrEmpty(surname))
                {
                    // Check for surname match (Case Insensitive)
                    bool isMatch = string.Equals(citizen.Surname.Trim(), surname.Trim(), StringComparison.OrdinalIgnoreCase);
                    
                    if (isMatch)
                    {
                        verificationResult = "VERIFIED";
                    }
                    else
                    {
                        verificationResult = "MISMATCH"; // Yellow
                        _logger.LogWarning("Surname Mismatch for ID {ID}. Claimed: {Claimed}, Official: {Official}", id, surname, citizen.Surname);
                    }
                }
                else
                {
                    // No surname provided, treat as simple lookup (Green if Alive)
                     verificationResult = "VERIFIED";
                }

                return Ok(new { 
                    found = true, 
                    status = status,
                    verificationResult = verificationResult,
                    data = new {
                        citizen.NationalID,
                        citizen.FirstName,
                        citizen.Surname,
                        citizen.DateOfBirth,
                        citizen.IsDeceased,
                        citizen.VerificationSource
                    }
                });
            }

            _logger.LogWarning("ID Not Found in Home Affairs: {ID}", id);
            return Ok(new { 
                found = false, 
                verificationResult = "NOT_FOUND",
                message = "Not found in Home Affairs Database" 
            });
        }
    }
}
