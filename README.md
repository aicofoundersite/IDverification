# CrossSetaDeduplicator - ID Verification & Duplicate Detector

## Overview
**CrossSetaDeduplicator** is MLX Ventures' submission for the W&R SETA Hackathon. It is a robust, Windows-based VB.NET application designed to handle **Learner Registration**, **Bulk ID Verification**, and **Cross-SETA Duplicate Detection**.

This lightweight solution uses **SQL Server 2019** and a polished **Windows Forms** interface to demonstrate exact and fuzzy matching algorithms for duplicate prevention.

---

## 🚀 Installation & Setup

### Prerequisites
Before running the application, ensure you have the following installed:
*   **Operating System:** Windows 10 or Windows 11.
*   **Database:** SQL Server 2019 (Developer or Express Edition).
*   **Development Environment:** Visual Studio 2022 (with .NET 6.0 Desktop Development workload).
*   **Framework:** .NET 6.0 Runtime (if running the compiled binary).

### Step-by-Step Setup
1.  **Clone the Repository:**
    ```bash
    git clone https://github.com/mlx-ventures/IDverification.git
    cd IDverification/CrossSetaDeduplicator
    ```

2.  **Database Configuration:**
    *   Open **SQL Server Management Studio (SSMS)**.
    *   Connect to your local SQL Server instance (e.g., `localhost` or `.\SQLEXPRESS`).
    *   **Step A:** Open and execute `db/CreateDatabase.sql` to create the `CrossSetaDB` database and tables.
    *   **Step B:** Open and execute `db/StoredProcedures.sql` to create the required Stored Procedures.
        *   *Note: This step is mandatory to ensure strict SQL 2019 compliance.*
    *   **Step C (Optional):** Open and execute `db/OptimizeDatabase.sql` to add performance indexes.

3.  **Application Configuration:**
    *   **Environment Variables (Recommended for Production):**
        *   Set `CROSS_SETA_DB_CONNECTION` to your SQL Server connection string.
        *   *Example:* `Server=192.168.1.100;Database=CrossSetaDB;User Id=sa;Password=YourPassword;`
    *   **Local Development:**
        *   The application defaults to `Server=localhost;Database=CrossSetaDB;Trusted_Connection=True;` if no environment variable is found.
        *   You can manually edit `src/CrossSetaDeduplicator/DataAccess/DatabaseHelper.vb` if needed.

---

## 🏭 Production Readiness

### Checklist
- [x] **Database Connectivity:** Supports Connection Pooling (via ADO.NET default) and Environment Variables.
- [x] **Data Integrity:** Primary Keys, Unique Constraints (NationalID), and Foreign Keys enforced.
- [x] **Performance:** Indexed searches for NationalID and Name (Fuzzy matching support).
- [x] **Security:** Uses Stored Procedures (`sp_InsertLearner`, `sp_FindPotentialDuplicates`) to prevent SQL Injection.
- [x] **Backup:** Backup script provided in `db/BackupDatabase.sql`.

### Known Limitations
1.  **Fuzzy Logic:** The fuzzy matching (Levenshtein) is currently handled in the application layer for flexibility. For datasets > 1 million rows, we recommend moving this to SQL Server Full-Text Search or SSIS Fuzzy Lookup.
2.  **Concurrency:** The Bulk Verification processes rows sequentially for UI responsiveness. Parallel processing can be enabled in `BulkVerification.vb` for higher throughput.

---

## 💻 Execution Instructions

### Running in Development Mode (Visual Studio)
1.  Open Visual Studio.
2.  Set `CrossSetaDeduplicator` as the **Startup Project**.
3.  Press **F5** or click **Start** to build and launch the application.
4.  The **Main Dashboard** should appear.

### Application Launch
Upon successful launch, you will see the **Main Dashboard** with:
*   **Sidebar Navigation:** Buttons for "New Learner Check", "Bulk Upload", and "Exit".
*   **Live Metrics:** Real-time counters for "Total Checks" and "Duplicates Found".
*   **Activity Log:** A scrolling log at the bottom showing system events.

---

## 📖 Application Behavior & Features

### 1. Dashboard & Demo Mode
*   **Live Demo Mode:** Toggle the checkbox in the bottom-left corner. This enables a guided narrative for judges, utilizing tooltips to explain the process.
*   **Seed Data:** Click the hidden/small "Seed Data" button in the top-right of the metrics panel to instantly populate the database with **50 sample records**, including known duplicates for testing.

### 2. New Learner Registration (Wizard)
Access via **"New Learner Check"**. A 3-step wizard guides you:
*   **Step 1: Identity Verification:** Enter a National ID. The system simulates a connection to Home Affairs (KYC).
    *   *Try ID ending in `999` for "Expired Document".*
    *   *Try ID `9505055000081` (Thabo Molefe) for a successful verification.*
*   **Step 2: Details:** Enter Name, Surname, DOB. (Auto-filled in Demo Mode).
*   **Step 3: Deduplication:** The system checks against the SQL database.
    *   **Green:** No duplicates found.
    *   **Red:** Duplicate detected (Exact ID match).
    *   **Orange:** Potential duplicate (Fuzzy name match).

### 3. Bulk ID Verification (Case 1)
Access via **"Bulk Upload"**.
*   **Browse:** Select the provided `SampleData.csv` file.
*   **Preview:** View the raw data in the grid.
*   **Process Batch:** Click to run the engine. Watch as rows color-code in real-time:
    *   🟢 **Green:** Verified.
    *   🔴 **Red:** Duplicate Found.
    *   🟠 **Orange:** KYC Failed / Invalid.
*   **Download Report:** Export the final results to a new CSV file.

---

## 🛠 Troubleshooting

*   **"Connection Refused" / SQL Error:**
    *   Ensure SQL Server is running.
    *   Check that the `CrossSetaDB` database exists.
    *   Verify the connection string in `DatabaseHelper.vb` matches your server name.

*   **Application Crashes on Start:**
    *   Ensure .NET 6.0 Runtime is installed.
    *   Run Visual Studio as Administrator if you encounter permission issues.

---

## 📝 Contact & Contribution

*   **Submission By:** Team MLX Ventures
*   **Event:** W&R SETA Hackathon
*   **Repository:** [GitHub Link]

For issues or questions, please open a GitHub Issue or contact the team lead.
