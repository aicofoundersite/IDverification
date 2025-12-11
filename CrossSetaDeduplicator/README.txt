CrossSetaDeduplicator - ID Verification Hackathon Submission
============================================================

Overview
--------
This solution demonstrates a VB.NET application for ID Verification and Deduplication, inspired by MOSIP logic.
It handles:
1. Individual Learner Registration with real-time duplicate checking.
2. Bulk ID Verification via CSV upload (Case 1).
3. Cross-SETA Duplicate Detection using Exact and Fuzzy matching logic (Case 2).

Prerequisites
-------------
- Windows OS (for running the WinForms app)
- Visual Studio 2022 (or compatible) with .NET 6.0 Desktop Development workload
- SQL Server 2019 (Developer or Express)

Setup Instructions
------------------
1. Database Setup:
   - Open SQL Server Management Studio (SSMS).
   - Execute the script found in `db/CreateDatabase.sql`.
   - This will create the `CrossSetaDB` database and necessary tables.

2. Application Configuration:
   - Open the solution `src/CrossSetaDeduplicator.sln` in Visual Studio.
   - Navigate to `src/CrossSetaDeduplicator/DataAccess/DatabaseHelper.vb`.
   - Update the `_connectionString` variable if your SQL Server instance name differs from `localhost`.

3. Build and Run:
   - Build the solution (Ctrl+Shift+B).
   - Run the application (F5).

Usage Guide
-----------
1. **Single Registration**:
   - Enter Learner details (National ID, Name, DOB, etc.).
   - Click "Check for Duplicates" to run the deduplication logic.
   - If no duplicates are found, click "Save" to persist the record.

2. **Bulk Verification (Case 1 & 2)**:
   - Click "Bulk ID Verification".
   - Select the provided `SampleData.csv` file.
   - The system will simulate KYC checks and run duplicate detection for each row.
   - A grid report will show the status (Verified, Duplicate, KYC Failed).

Architecture
------------
- **UI Layer**: VB.NET Windows Forms.
- **Service Layer**: `DeduplicationService.vb` handles the business logic (Exact Match, Levenshtein Fuzzy Match).
- **Data Layer**: `DatabaseHelper.vb` manages SQL Server interactions using ADO.NET.
- **Database**: SQL Server 2019 with optimized indexing for Name and NationalID.

Files Included
--------------
- src/ : Source code for the VB.NET application.
- db/ : SQL scripts for database creation.
- SampleData.csv : Test data for bulk upload.
- README.txt : This file.

Contact
-------
Team MLX Ventures
