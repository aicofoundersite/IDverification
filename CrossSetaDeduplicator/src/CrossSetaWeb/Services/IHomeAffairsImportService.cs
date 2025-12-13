using System.Threading.Tasks;
using CrossSetaWeb.Models;

namespace CrossSetaWeb.Services
{
    public interface IHomeAffairsImportService
    {
        Task<ImportResult> ImportFromUrlAsync(string url);
        ImportResult ImportFromContent(string content);
    }
}
