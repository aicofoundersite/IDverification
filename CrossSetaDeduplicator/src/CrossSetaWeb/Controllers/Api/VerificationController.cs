using Microsoft.AspNetCore.Mvc;
using CrossSetaWeb.DataAccess;
using CrossSetaWeb.Models;

namespace CrossSetaWeb.Controllers.Api
{
    [Route("api/[controller]")]
    [ApiController]
    public class VerificationController : ControllerBase
    {
        private readonly DatabaseHelper _dbHelper;

        public VerificationController()
        {
            _dbHelper = new DatabaseHelper();
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
        public IActionResult VerifyHomeAffairs(string id)
        {
            if (string.IsNullOrEmpty(id)) return BadRequest("ID is required");

            var citizen = _dbHelper.GetHomeAffairsCitizen(id);
            if (citizen != null)
            {
                return Ok(new { 
                    found = true, 
                    status = citizen.IsDeceased ? "Deceased" : "Alive", 
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
            return Ok(new { found = false, message = "Not found in Home Affairs Database" });
        }
    }
}
