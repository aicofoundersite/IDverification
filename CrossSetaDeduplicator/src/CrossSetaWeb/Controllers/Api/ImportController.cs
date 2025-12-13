using Microsoft.AspNetCore.Mvc;
using CrossSetaWeb.Services;
using CrossSetaWeb.DataAccess;
using System.Threading.Tasks;

namespace CrossSetaWeb.Controllers.Api
{
    [Route("api/[controller]")]
    [ApiController]
    public class ImportController : ControllerBase
    {
        private readonly HomeAffairsImportService _importService;

        public ImportController()
        {
            // Manual injection as per existing pattern
            var dbHelper = new DatabaseHelper();
            _importService = new HomeAffairsImportService(dbHelper);
        }

        [HttpPost("trigger")]
        public async Task<IActionResult> TriggerImport()
        {
            // Security: Simulate RBAC (Attribute-based access control would be [Authorize(Policy="Admin")])
            // For now, we allow it for demonstration/hackathon purposes.

            // Source: Using the Google Sheet CSV as the "External Database Source" 
            // since the provided .bak file requires SQL Server Restoration which is not feasible in this environment.
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
    }
}
