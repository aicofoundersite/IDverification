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

            var learners = _dbHelper.GetAllLearners();
            _logger.LogInformation($"Retrieved {learners.Count} learners from database.");

            var result = new DatabaseValidationResult
            {
                TotalRecords = learners.Count
            };

            if (jobId != null) _progressService.UpdateProgress(jobId, 0, learners.Count, "Validating...");

            int processed = 0;
            foreach (var learner in learners)
            {
                processed++;
                // Update progress every 10 records or if total is small
                if (jobId != null && (processed % 10 == 0 || learners.Count < 50))
                {
                    _progressService.UpdateProgress(jobId, processed, learners.Count, $"Processing {processed}/{learners.Count}");
                }

                var detail = new ValidationDetail
                {
                    NationalID = learner.NationalID,
                    FirstName = learner.FirstName,
                    LastName = learner.LastName
                };

                // 1. Basic format check (Luhn)
                if (!BulkRegistrationService.IsValidLuhn(learner.NationalID))
                {
                     detail.Status = "InvalidFormat";
                     detail.Message = "Invalid ID Number format (Luhn check failed).";
                     result.InvalidFormatCount++;
                     result.Details.Add(detail);
                     continue;
                }

                // 2. Check Home Affairs
                var citizen = _dbHelper.GetHomeAffairsCitizen(learner.NationalID);

                if (citizen == null)
                {
                    detail.Status = "NotFound";
                    detail.Message = "Identity Number not found in Home Affairs database.";
                    result.NotFoundCount++;
                }
                else if (citizen.IsDeceased)
                {
                    detail.Status = "Deceased";
                    detail.Message = "Learner is marked as DECEASED in Home Affairs database.";
                    result.DeceasedCount++;
                }
                else
                {
                    // 3. Surname Check
                    if (!string.Equals(learner.LastName?.Trim(), citizen.Surname?.Trim(), StringComparison.OrdinalIgnoreCase))
                    {
                        detail.Status = "SurnameMismatch";
                        detail.Message = $"Surname Mismatch. Database: '{learner.LastName}', Home Affairs: '{citizen.Surname}'";
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
    }
}
