using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using CrossSetaWeb.DataAccess;
using CrossSetaWeb.Models;
using Microsoft.AspNetCore.Http;

namespace CrossSetaWeb.Services
{
    public class BulkRegistrationService
    {
        private readonly DatabaseHelper _dbHelper;

        public BulkRegistrationService(DatabaseHelper dbHelper)
        {
            _dbHelper = dbHelper;
        }

        public BulkImportResult ProcessBulkFile(IFormFile file)
        {
            var result = new BulkImportResult();
            var learners = new List<LearnerModel>();

            try
            {
                using (var reader = new StreamReader(file.OpenReadStream()))
                {
                    string line;
                    bool isHeader = true;
                    int rowNumber = 0;

                    while ((line = reader.ReadLine()) != null)
                    {
                        rowNumber++;
                        if (string.IsNullOrWhiteSpace(line)) continue;
                        if (isHeader)
                        {
                            isHeader = false;
                            continue;
                        }

                        try
                        {
                            var learner = ParseLine(line);
                            learners.Add(learner);
                            result.SuccessCount++;
                        }
                        catch (Exception ex)
                        {
                            result.Errors.Add($"Row {rowNumber}: {ex.Message}");
                            result.FailureCount++;
                        }
                    }
                }

                if (learners.Count > 0)
                {
                    _dbHelper.BatchInsertLearners(learners);
                }
            }
            catch (Exception ex)
            {
                result.Errors.Add($"Fatal Error: {ex.Message}");
            }

            return result;
        }

        private LearnerModel ParseLine(string line)
        {
            // Expected CSV Format:
            // NationalID,FirstName,LastName,DateOfBirth(yyyy-MM-dd),Gender,Email,Phone,SetaName
            var parts = line.Split(',');

            if (parts.Length < 3) throw new Exception("Insufficient columns. Required: NationalID, FirstName, LastName");

            var learner = new LearnerModel
            {
                NationalID = parts[0].Trim(),
                FirstName = parts[1].Trim(),
                LastName = parts[2].Trim(),
                IsVerified = false, // Bulk imported, needs verification or assumed verified? Let's say false for now or true if trusted.
                PopiActConsent = true, // Assumed for bulk uploads from partners
                PopiActDate = DateTime.Now
            };

            // Optional fields parsing
            if (parts.Length > 3 && DateTime.TryParse(parts[3], out DateTime dob))
                learner.DateOfBirth = dob;
            
            if (parts.Length > 4) learner.Gender = parts[4].Trim();
            if (parts.Length > 5) learner.EmailAddress = parts[5].Trim();
            if (parts.Length > 6) learner.PhoneNumber = parts[6].Trim();
            if (parts.Length > 7) learner.SetaName = parts[7].Trim();

            // Validate ID (Luhn)
            if (!IsValidLuhn(learner.NationalID))
                throw new Exception($"Invalid ID Number: {learner.NationalID}");

            return learner;
        }

        private bool IsValidLuhn(string id)
        {
            if (string.IsNullOrEmpty(id)) return false;
            id = id.Trim();
            if (id.Length != 13 || !long.TryParse(id, out _)) return false;

            int sum = 0;
            bool alternate = false;
            for (int i = id.Length - 1; i >= 0; i--)
            {
                char c = id[i];
                int n = int.Parse(c.ToString());
                if (alternate)
                {
                    n *= 2;
                    if (n > 9) n = (n % 10) + 1;
                }
                sum += n;
                alternate = !alternate;
            }
            return (sum % 10 == 0);
        }
    }

    public class BulkImportResult
    {
        public int SuccessCount { get; set; }
        public int FailureCount { get; set; }
        public List<string> Errors { get; set; } = new List<string>();
    }
}
