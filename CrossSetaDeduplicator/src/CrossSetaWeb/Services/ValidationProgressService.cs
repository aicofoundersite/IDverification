using System;
using System.Collections.Concurrent;
using System.Linq;

namespace CrossSetaWeb.Services
{
    public class ValidationProgress
    {
        public int Total { get; set; }
        public int Processed { get; set; }
        public int Percentage => Total == 0 ? 0 : (int)((double)Processed / Total * 100);
        public string Status { get; set; }
        public bool IsComplete { get; set; }
        public DatabaseValidationResult Result { get; set; }
    }

    public interface IValidationProgressService
    {
        void StartValidation(string jobId);
        void UpdateProgress(string jobId, int processed, int total, string status);
        void CompleteValidation(string jobId, DatabaseValidationResult result);
        ValidationProgress GetProgress(string jobId);
    }

    public class ValidationProgressService : IValidationProgressService
    {
        private static readonly ConcurrentDictionary<string, ValidationProgress> _jobs = new ConcurrentDictionary<string, ValidationProgress>();

        public void StartValidation(string jobId)
        {
            _jobs[jobId] = new ValidationProgress 
            { 
                Total = 0, 
                Processed = 0, 
                Status = "Starting...", 
                IsComplete = false 
            };
        }

        public void UpdateProgress(string jobId, int processed, int total, string status)
        {
            if (_jobs.TryGetValue(jobId, out var progress))
            {
                progress.Processed = processed;
                progress.Total = total;
                progress.Status = status;
            }
        }

        public void CompleteValidation(string jobId, DatabaseValidationResult result)
        {
            if (_jobs.TryGetValue(jobId, out var progress))
            {
                progress.IsComplete = true;
                progress.Status = "Completed";
                progress.Result = result;
                progress.Processed = progress.Total; // Ensure 100%
            }
        }

        public ValidationProgress GetProgress(string jobId)
        {
            if (_jobs.TryGetValue(jobId, out var progress))
            {
                return progress;
            }
            return null;
        }
    }
}
