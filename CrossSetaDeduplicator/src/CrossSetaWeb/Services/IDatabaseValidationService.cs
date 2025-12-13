namespace CrossSetaWeb.Services
{
    public interface IDatabaseValidationService
    {
        DatabaseValidationResult ValidateDatabase(string jobId = null);
    }
}
