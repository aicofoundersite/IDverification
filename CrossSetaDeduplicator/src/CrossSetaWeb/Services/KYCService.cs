using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using CrossSetaWeb.Models;

namespace CrossSetaWeb.Services
{
    public class KYCResult
    {
        public bool IsSuccess { get; set; }
        public string DocumentType { get; set; }
        public string ErrorMessage { get; set; }
        public string ExtractedNationalID { get; set; }
        public string ExtractedSurname { get; set; }
    }

    public class KYCService : IKYCService
    {
        private readonly string _uploadPath;

        public KYCService()
        {
            _uploadPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", "kyc");
            if (!Directory.Exists(_uploadPath))
            {
                Directory.CreateDirectory(_uploadPath);
            }
        }

        public async Task<KYCResult> VerifyDocumentAsync(IFormFile file, string claimedNationalID)
        {
            if (file == null || file.Length == 0)
            {
                return new KYCResult { IsSuccess = false, ErrorMessage = "No file uploaded." };
            }

            // 1. Save File Securely (Rename to prevent collisions)
            string fileName = $"{claimedNationalID}_{DateTime.Now.Ticks}{Path.GetExtension(file.FileName)}";
            string fullPath = Path.Combine(_uploadPath, fileName);

            using (var stream = new FileStream(fullPath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            // 2. Mock Verification Logic (Linux/Docker Environment)
            // In a real scenario, we would call the Doubango SDK binary here.
            // Since we are running in a Linux Container without the specific binaries,
            // we will simulate the "Verification" step.
            
            // Simulation: Assume if file is uploaded, it's a valid ID for this Hackathon demo.
            await Task.Delay(1000); // Simulate processing time

            return new KYCResult 
            { 
                IsSuccess = true, 
                DocumentType = "South African ID Document",
                ExtractedNationalID = claimedNationalID, // Simulate successful extraction
                ExtractedSurname = "SimulatedSurname"
            };
        }
    }
}
