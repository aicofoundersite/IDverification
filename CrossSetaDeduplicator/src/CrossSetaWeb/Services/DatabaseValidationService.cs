using System;
using System.Collections.Generic;
using CrossSetaWeb.DataAccess;
using CrossSetaWeb.Models;
using Microsoft.Extensions.Logging;

namespace CrossSetaWeb.Services
{
    public class DatabaseValidationResult
    {
        public int TotalRecords { get; set; }
        public int ValidCount { get; set; }
        public int DeceasedCount { get; set; }
        public int NotFoundCount { get; set; }
        public int SurnameMismatchCount { get; set; }
        public int InvalidFormatCount { get; set; }
        public string ReportFileName { get; set; }
        public List<ValidationDetail> Details { get; set; } = new List<ValidationDetail>();
    }

    public class ValidationDetail
    {
        public string NationalID { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Status { get; set; } // Valid, Deceased, NotFound, SurnameMismatch, InvalidFormat
        public string Message { get; set; }
    }

    public class DatabaseValidationService : IDatabaseValidationService
    {
        private readonly IDatabaseHelper _dbHelper;
        private readonly ILogger<DatabaseValidationService> _logger;
        private readonly IValidationProgressService _progressService;

        public DatabaseValidationService(IDatabaseHelper dbHelper, ILogger<DatabaseValidationService> logger, IValidationProgressService progressService)
        {
            _dbHelper = dbHelper;
            _logger = logger;
            _progressService = progressService;
        }

        public DatabaseValidationResult ValidateDatabase(string jobId = null)
        {
            _logger.LogInformation("Starting database validation against Home Affairs records.");
            if (jobId != null) _progressService.UpdateProgress(jobId, 0, 0, "Fetching Learners...");

            // Optimized: Fetch all data with Home Affairs join in one query
            var validationResults = _dbHelper.GetLearnerValidationResults();
            _logger.LogInformation($"Retrieved {validationResults.Count} validation records from database.");

            // Fallback for empty DB (Development/Demo mode)
            if (validationResults.Count <= 10)
            {
                var path = System.IO.Path.Combine(System.IO.Directory.GetCurrentDirectory(), "wwwroot", "uploads", "LearnerData.csv");
                var fallbackList = new List<LearnerValidationResult>();
                try
                {
                    using (var reader = new System.IO.StreamReader(path))
                    {
                        string line;
                        bool header = true;
                        while ((line = reader.ReadLine()) != null)
                        {
                            if (string.IsNullOrWhiteSpace(line)) continue;
                            if (header) { header = false; continue; }
                            var parts = ParseCsvLine(line);
                            if (parts.Count < 4) continue;
                            var id = parts[3].Trim();
                            if (!BulkRegistrationService.IsValidLuhn(id)) continue;
                            
                            // Mock validation for fallback data
                            fallbackList.Add(new LearnerValidationResult 
                            { 
                                FirstName = parts[0].Trim(), 
                                LastName = parts[1].Trim(), 
                                NationalID = id,
                                IsFoundInHomeAffairs = false // Default to not found for local file
                            });
                        }
                    }
                }
                catch {}
                if (fallbackList.Count > 0) validationResults = fallbackList;
            }

            var result = new DatabaseValidationResult
            {
                TotalRecords = validationResults.Count
            };

            if (jobId != null) _progressService.UpdateProgress(jobId, 0, validationResults.Count, "Validating...");

            int processed = 0;
            foreach (var item in validationResults)
            {
                processed++;
                // Update progress every 1000 records to reduce overhead (was 10)
                if (jobId != null && (processed % 1000 == 0 || validationResults.Count < 50))
                {
                    _progressService.UpdateProgress(jobId, processed, validationResults.Count, $"Processing {processed}/{validationResults.Count}");
                }

                var detail = new ValidationDetail
                {
                    NationalID = item.NationalID,
                    FirstName = item.FirstName,
                    LastName = item.LastName
                };

                // 1. Basic format check (Luhn)
                if (!BulkRegistrationService.IsValidLuhn(item.NationalID))
                {
                     detail.Status = "InvalidFormat";
                     detail.Message = "Invalid ID Number format (Luhn check failed).";
                     result.InvalidFormatCount++;
                     result.Details.Add(detail);
                     continue;
                }

                // 2. Check Home Affairs
                if (!item.IsFoundInHomeAffairs)
                {
                    detail.Status = "NotFound";
                    detail.Message = "Identity Number not found in Home Affairs database.";
                    result.NotFoundCount++;
                }
                else if (item.IsDeceased)
                {
                    detail.Status = "Deceased";
                    detail.Message = "Learner is marked as DECEASED in Home Affairs database.";
                    result.DeceasedCount++;
                }
                else
                {
                    // 3. Surname Check
                    if (!string.Equals(item.LastName?.Trim(), item.HomeAffairsSurname?.Trim(), StringComparison.OrdinalIgnoreCase))
                    {
                        detail.Status = "SurnameMismatch";
                        detail.Message = $"Surname Mismatch. Database: '{item.LastName}', Home Affairs: '{item.HomeAffairsSurname}'";
                        result.SurnameMismatchCount++;
                    }
                    else
                    {
                        detail.Status = "Valid";
                        detail.Message = "Verified against Home Affairs (Alive).";
                        result.ValidCount++;
                    }
                }

                result.Details.Add(detail);
            }

            _logger.LogInformation("Database validation completed. Total: {Total}, Valid: {Valid}, Deceased: {Deceased}, NotFound: {NotFound}, SurnameMismatch: {SurnameMismatch}, InvalidFormat: {InvalidFormat}", 
                result.TotalRecords, result.ValidCount, result.DeceasedCount, result.NotFoundCount, result.SurnameMismatchCount, result.InvalidFormatCount);
            
            // Note: We don't complete here anymore, Controller does it to add ReportFileName
            // if (jobId != null) _progressService.CompleteValidation(jobId, result);

            return result;
        }
        private List<string> ParseCsvLine(string line)
        {
            var values = new List<string>();
            bool inQuotes = false;
            var current = new System.Text.StringBuilder();
            for (int i = 0; i < line.Length; i++)
            {
                char c = line[i];
                if (c == '"')
                {
                    if (inQuotes && i + 1 < line.Length && line[i + 1] == '"') { current.Append('"'); i++; }
                    else { inQuotes = !inQuotes; }
                }
                else if (c == ',' && !inQuotes)
                {
                    values.Add(current.ToString());
                    current.Clear();
                }
                else current.Append(c);
            }
            values.Add(current.ToString());
            return values;
        }
    }
}
