using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using CrossSetaWeb.DataAccess;
using CrossSetaWeb.Models;

namespace CrossSetaWeb.Services
{
    public class HomeAffairsImportService
    {
        private readonly DatabaseHelper _dbHelper;
        private readonly HttpClient _httpClient;

        public HomeAffairsImportService(DatabaseHelper dbHelper)
        {
            _dbHelper = dbHelper;
            _httpClient = new HttpClient();
        }

        public async Task<ImportResult> ImportFromUrlAsync(string url)
        {
            try
            {
                // 1. Fetch Data (TLS 1.2+ is default in .NET Core)
                var response = await _httpClient.GetAsync(url);
                response.EnsureSuccessStatusCode();
                
                string content = await response.Content.ReadAsStringAsync();
                
                // 2. Parse & Validate
                var validRecords = new List<HomeAffairsCitizen>();
                var errors = new List<string>();

                using (var reader = new StringReader(content))
                {
                    // Basic CSV parsing
                    string line;
                    bool isHeader = true;
                    while ((line = reader.ReadLine()) != null)
                    {
                        if (isHeader) { isHeader = false; continue; } // Skip header
                        
                        if (string.IsNullOrWhiteSpace(line)) continue;

                        // CSV Split (Handling basic commas)
                        var parts = line.Split(','); 
                        if (parts.Length < 4) continue;

                        try 
                        {
                            var record = ParseAndValidate(parts);
                            validRecords.Add(record);
                        }
                        catch (Exception ex)
                        {
                            errors.Add($"Row Error: {ex.Message} | Data: {line}");
                        }
                    }
                }

                // 3. Batch Insert with Transaction
                if (validRecords.Count > 0)
                {
                    _dbHelper.InitializeHomeAffairsTable(); // Ensure table exists
                    _dbHelper.BatchImportHomeAffairsData(validRecords);
                }

                return new ImportResult 
                { 
                    Success = true, 
                    RecordsProcessed = validRecords.Count, 
                    Errors = errors 
                };
            }
            catch (Exception ex)
            {
                return new ImportResult { Success = false, Errors = new List<string> { ex.Message } };
            }
        }

        private HomeAffairsCitizen ParseAndValidate(string[] parts)
        {
            // Input Mapping: First Name, Surname, Date of Birth, Identity Number
            // Index: 0, 1, 2, 3
            
            // Sanitization (OWASP - basic HTML encoding/stripping)
            string firstName = Sanitize(parts[0]);
            string surname = Sanitize(parts[1]);
            string dobStr = parts[2].Trim();
            string idNumber = parts[3].Trim();

            // Validation 1: ID Format & Luhn
            if (!IsValidLuhn(idNumber)) throw new Exception("Invalid ID Number (Luhn Check Failed)");

            // Validation 2: Date
            // Try generic formats
            if (!DateTime.TryParse(dobStr, out DateTime dob)) 
            {
                 throw new Exception("Invalid Date of Birth format");
            }
            
            return new HomeAffairsCitizen
            {
                NationalID = idNumber,
                FirstName = firstName,
                Surname = surname,
                DateOfBirth = dob,
                IsDeceased = false, // Default to Alive unless marked otherwise
                VerificationSource = "GoogleSheet_Import"
            };
        }

        private string Sanitize(string input)
        {
            if (string.IsNullOrEmpty(input)) return string.Empty;
            // Remove potential scripts and trim
            return Regex.Replace(input, "<.*?>", String.Empty).Trim();
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

    public class ImportResult
    {
        public bool Success { get; set; }
        public int RecordsProcessed { get; set; }
        public List<string> Errors { get; set; }
    }
}
