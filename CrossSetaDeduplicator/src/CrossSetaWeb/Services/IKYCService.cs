using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace CrossSetaWeb.Services
{
    public interface IKYCService
    {
        Task<KYCResult> VerifyDocumentAsync(IFormFile file, string claimedNationalID);
    }
}
