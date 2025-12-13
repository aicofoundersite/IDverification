using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using CrossSetaWeb.DataAccess;
using CrossSetaWeb.Models;
using Microsoft.AspNetCore.Http;

using Microsoft.Extensions.Logging;

namespace CrossSetaWeb.Services
{
    public class BulkRegistrationService
    {
        private readonly DatabaseHelper _dbHelper;
        private readonly ILogger<BulkRegistrationService> _logger;

        public BulkRegistrationService(DatabaseHelper dbHelper, ILogger<BulkRegistrationService> logger)
        {
            _dbHelper = dbHelper;
            _logger = logger;
        }

        public BulkImportResult ProcessBulkFile(IFormFile file)
        {
            _logger.LogInformation("Starting bulk import for file: {FileName}, Size: {Length}", file.FileName, file.Length);
            var result = new BulkImportResult();
            var validLearners = new List<LearnerModel>();
            var rowMap = new Dictionary<string, int>(); // Map NationalID to RowNumber for error reporting

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
                        
                        // Heuristic check: if the first line contains "Identity Number", treat as header
                        if (isHeader)
                        {
                            if (line.Contains("Identity Number", StringComparison.OrdinalIgnoreCase) || 
                                line.Contains("First Name", StringComparison.OrdinalIgnoreCase))
                            {
                                isHeader = false;
                                continue;
                            }
                            isHeader = false; // If not header-like, treat as data? Or just skip 1st line always?
                            // Safe bet: Always skip first line if it's the very first line read.
                            // But if user uploads NO header, we lose data.
                            // Given the requirement "use the csv format in this file", which HAS headers, we stick to skipping first line.
                            continue;
                        }

                        try
                        {
                            var learner = ParseLine(line, rowNumber);
                            
                            // Check for duplicates within the file itself
                            if (rowMap.ContainsKey(learner.NationalID))
                            {
                                string msg = "Duplicate ID within the same file.";
                                result.ErrorDetails.Add(new BulkErrorDetail 
                                { 
                                    RowNumber = rowNumber, 
                                    NationalID = learner.NationalID, 
                                    Message = msg 
                                });
                                result.FailureCount++;
                                _logger.LogWarning("Row {RowNumber}: {Message}", rowNumber, msg);
                                continue;
                            }

                            validLearners.Add(learner);
                            rowMap[learner.NationalID] = rowNumber;
                        }
                        catch (Exception ex)
                        {
                            result.ErrorDetails.Add(new BulkErrorDetail 
                            { 
                                RowNumber = rowNumber, 
                                NationalID = "Unknown", 
                                Message = ex.Message 
                            });
                            result.FailureCount++;
                            _logger.LogWarning("Row {RowNumber} Parse Error: {Message}", rowNumber, ex.Message);
                        }
                    }
                }

                if (validLearners.Count > 0)
                {
                    _logger.LogInformation("Parsed {Count} valid records. Attempting DB insertion.", validLearners.Count);
                    
                    // Attempt DB Insertion
                    var dbErrors = _dbHelper.BatchInsertLearners(validLearners);

                    // Map DB errors back to results
                    foreach (var error in dbErrors)
                    {
                        int row = rowMap.ContainsKey(error.NationalID) ? rowMap[error.NationalID] : 0;
                        result.ErrorDetails.Add(new BulkErrorDetail
                        {
                            RowNumber = row,
                            NationalID = error.NationalID,
                            Message = error.Message
                        });
                        result.FailureCount++;
                        _logger.LogError("DB Error Row {RowNumber} (ID: {NationalID}): {Message}", row, error.NationalID, error.Message);
                    }

                    // Success count is total valid sent - total db errors
                    result.SuccessCount = validLearners.Count - dbErrors.Count;
                }
                
                _logger.LogInformation("Bulk import completed. Success: {Success}, Failed: {Failed}", result.SuccessCount, result.FailureCount);
            }
            catch (Exception ex)
            {
                string fatalMsg = $"Fatal Error: {ex.Message}";
                result.ErrorDetails.Add(new BulkErrorDetail { RowNumber = 0, Message = fatalMsg });
                _logger.LogCritical(ex, "Fatal error during bulk import.");
            }

            return result;
        }

        private LearnerModel ParseLine(string line, int rowNumber)
        {
            // Robust CSV Parsing to handle quotes
            var parts = ParseCsvLine(line);

            // Expected CSV Format from Google Sheet:
            // First Name / s, Surname, Date of Birth, Identity Number, Target #, Intervention Name, Period
            // Index: 0, 1, 2, 3
            if (parts.Count < 4) throw new Exception("Insufficient columns. Required: First Name, Surname, Date of Birth, Identity Number");

            var learner = new LearnerModel
            {
                FirstName = parts[0].Trim(),
                LastName = parts[1].Trim(),
                NationalID = parts[3].Trim(), // Identity Number is at index 3
                IsVerified = false,
                PopiActConsent = true,
                PopiActDate = DateTime.Now
            };

            // Validate ID (Luhn)
            if (!IsValidLuhn(learner.NationalID))
                throw new Exception($"Invalid ID Number: {learner.NationalID}");

            // Date Parsing (Format: dd/MM/yy)
            string dobStr = parts[2].Trim();
            if (!string.IsNullOrWhiteSpace(dobStr))
            {
                // Try specific formats matching the sheet (dd/MM/yy or dd/MM/yyyy)
                string[] formats = { "dd/MM/yy", "dd/MM/yyyy", "yyyy-MM-dd" };
                if (DateTime.TryParseExact(dobStr, formats, System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None, out DateTime dob))
                {
                    learner.DateOfBirth = dob;
                }
                else
                {
                    // Fallback to generic parse if specific formats fail
                    if (DateTime.TryParse(dobStr, out dob))
                        learner.DateOfBirth = dob;
                    else
                        throw new Exception($"Invalid Date Format: {dobStr} (Expected dd/MM/yy)");
                }
            }
            
            // Default values for fields not in this specific CSV format
            learner.Gender = "Unknown"; // Not in CSV
            
            return learner;
        }

        private List<string> ParseCsvLine(string line)
        {
            var values = new List<string>();
            bool inQuotes = false;
            var currentValue = new StringBuilder();

            for (int i = 0; i < line.Length; i++)
            {
                char c = line[i];
                if (c == '\"')
                {
                    if (inQuotes && i + 1 < line.Length && line[i + 1] == '\"')
                    {
                        currentValue.Append('\"'); // Escaped quote
                        i++;
                    }
                    else
                    {
                        inQuotes = !inQuotes;
                    }
                }
                else if (c == ',' && !inQuotes)
                {
                    values.Add(currentValue.ToString());
                    currentValue.Clear();
                }
                else
                {
                    currentValue.Append(c);
                }
            }
            values.Add(currentValue.ToString());
            return values;
        }

        private bool IsValidEmail(string email)
        {
            try {
                var addr = new System.Net.Mail.MailAddress(email);
                return addr.Address == email;
            }
            catch { return false; }
        }

        private bool IsValidPhone(string phone)
        {
            // Simple check: starts with 0 or +27, contains only digits and spaces, length 10-15
            return Regex.IsMatch(phone, @"^(\+27|0)[0-9\s]{8,15}$");
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
        public List<BulkErrorDetail> ErrorDetails { get; set; } = new List<BulkErrorDetail>();
        // Compatibility property if needed, maps details to strings
        public List<string> Errors => ErrorDetails.Select(e => $"Row {e.RowNumber}: {e.Message}").ToList();
    }

    public class BulkErrorDetail
    {
        public int RowNumber { get; set; }
        public string NationalID { get; set; }
        public string Message { get; set; }
    }
}
