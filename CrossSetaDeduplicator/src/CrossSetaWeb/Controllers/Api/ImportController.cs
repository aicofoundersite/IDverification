using Microsoft.AspNetCore.Mvc;
using CrossSetaLogic.Services;
using CrossSetaLogic.DataAccess;
using System.Threading.Tasks;
using System.IO;

namespace CrossSetaWeb.Controllers.Api
{
    [Route("api/[controller]")]
    [ApiController]
    public class ImportController : ControllerBase
    {
        private readonly IHomeAffairsImportService _importService;
        private readonly IBulkRegistrationService _bulkService;

        public ImportController(IHomeAffairsImportService importService, IBulkRegistrationService bulkService)
        {
            _importService = importService;
            _bulkService = bulkService;
        }

        [HttpPost("trigger")]
        public async Task<IActionResult> TriggerImport()
        {
            // Security: Simulate RBAC (Attribute-based access control would be [Authorize(Policy="Admin")])
            // For now, we allow it for demonstration/hackathon purposes.

            // Source: Using the Google Sheet CSV as the "External Database Source" 
            // since the provided .bak file (5.3GB) requires SQL Server Restoration and cannot be imported via the CSV parser.
            string csvUrl = "https://docs.google.com/spreadsheets/d/1eQjxSsuOuXU20xG0gGgmR0Agn7WvudJd/export?format=csv&gid=572729852";
            
            var result = await _importService.ImportFromUrlAsync(csvUrl);
            
            if (result.Success)
            {
                return Ok(new { Message = "Home Affairs Database Import Successful", Details = result });
            }
            else
            {
                return BadRequest(new { Message = "Import Failed", Errors = result.Errors });
            }
        }

        [HttpPost("seed-learners")]
        public IActionResult SeedLearners()
        {
            var webRoot = Directory.GetCurrentDirectory();
            var path = Path.Combine(webRoot, "wwwroot", "uploads", "LearnerData.csv");
            _bulkService.SeedLearners(path);
            return Ok(new { Message = "Learner seeding triggered", Path = path });
        }
    }
}
