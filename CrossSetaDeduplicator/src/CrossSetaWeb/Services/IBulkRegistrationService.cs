using CrossSetaWeb.Models;
using Microsoft.AspNetCore.Http;

namespace CrossSetaWeb.Services
{
    public interface IBulkRegistrationService
    {
        BulkImportResult ProcessBulkFile(IFormFile file);
    }
}
