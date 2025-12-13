# CrossSetaDeduplicator - Intelligent ID Verification & Deduplication System

## 📋 Project Overview
**CrossSetaDeduplicator** is a comprehensive identity verification solution for W&RSETA, consisting of a **Web Portal** (ASP.NET Core) and a **Desktop Application** (Windows Forms). It provides a secure, real-time mechanism to validate South African ID numbers, prevent duplicate learner registrations, and ensure data integrity across the SETA landscape.

This solution is built on a strict **N-Tier Architecture**, utilizing **SQL Server 2019** for robust data management and audit compliance.

### ☁️ Cloud Deployment
The application is fully containerized using **Docker** and deployed to **Fly.io** for high availability and global scalability.

*   **Live Web Portal**: [https://cross-seta-web-17655.fly.dev/](https://cross-seta-web-17655.fly.dev/)
*   **Deployment Guide**: 👉 **[Read the Fly.io Deployment Guide](DEPLOY_TO_FLY.md)**
*   **Code**: [Dockerfile](CrossSetaDeduplicator/src/CrossSetaWeb/Dockerfile)

### 📦 Home Affairs Database Import & Management
To meet the requirement of a local, high-performance copy of the Home Affairs database, the system includes a robust import engine.

1.  **Trigger Import**:
    *   API Endpoint: `POST /api/import/trigger`
    *   Source: Imports from a secure Google Sheet (simulating the 5.3GB .bak file for the hackathon environment).
    *   Process: Fetches CSV -> Validates (Luhn/Dates) -> Sanitizes -> Batch Inserts (Transaction Safe).
    *   **Code**: [ImportController.cs](CrossSetaDeduplicator/src/CrossSetaWeb/Controllers/Api/ImportController.cs)
2.  **Validation Script**:
    *   A Node.js script is included to verify the integrity and performance of the imported data.
    *   Run: `node src/CrossSetaWeb/scripts/validate_home_affairs.js`
    *   **Performance**: Optimized for **sub-100ms** query response times (Verified avg: ~15ms).
    *   **Code**: [validate_home_affairs.js](CrossSetaDeduplicator/src/CrossSetaWeb/scripts/validate_home_affairs.js)

---

## 🎯 Core Capabilities (The "Must-Haves")

### 1. 💻 Modern React Frontend
A brand new, responsive **React + TypeScript** frontend provides a seamless user experience.
*   **Quick Verification**: Search bar for instant Learner ID verification.
*   **Status Indicators**: Visual cues (Alive/Deceased) with clear "Traffic Light" coloring.
*   **Registration Integration**: One-click access to User and Learner registration workflows.
*   **Code**: [App.tsx](CrossSetaDeduplicator/src/CrossSetaWeb/ClientApp/src/App.tsx)

### 2. ✅ Input Validation
*   **South African ID Standard**: Implements the **Luhn Algorithm (Modulus 10)** to mathematically validate ID numbers before they leave the client.
    *   **Code**: [LuhnAttribute.cs](CrossSetaDeduplicator/src/CrossSetaWeb/Validation/LuhnAttribute.cs)
*   **Format Enforcement**: Strict input masking ensures 13-digit numeric compliance.
*   **Data Integrity**: Mandatory checks for Name and Surname to ensure complete records.
    *   **Code**: [LearnerModel.cs](CrossSetaDeduplicator/src/CrossSetaWeb/Models/LearnerModel.cs)

### 2. ⚡ Real-Time Verification (Traffic Light Protocol)
Connects to an external trusted data source (simulating Department of Home Affairs) to verify identity status in real-time.
*   🟢 **GREEN (Verified)**: ID exists, is "Alive", and the Surname matches the official record.
*   🟡 **YELLOW (Warning)**: ID is valid, but the Surname provided does not match the official record (potential marriage/typo).
*   🔴 **RED (Invalid/Fraud)**: ID does not exist, or the person is marked as "Deceased".
*   **Code**: [VerificationController.cs](CrossSetaDeduplicator/src/CrossSetaWeb/Controllers/Api/VerificationController.cs)

### 3. 🔒 Audit Trail & Compliance
*   **Full Traceability**: Every single verification attempt is logged in the `ExternalVerificationLog` table.
*   **Who, When, What**: Logs the **Timestamp**, **User** (System User), **Source** (e.g., HomeAffairs, KYC_SDK), and **Result Status**.
*   **POPIA Compliance**: Mandatory "Consent" checkbox explicitly required before any verification can proceed.
*   **Code**: [DatabaseHelper.cs](CrossSetaDeduplicator/src/CrossSetaWeb/DataAccess/DatabaseHelper.cs)

### 4. 🗄️ Home Affairs Database Integration
*   **Secure Import**: Capabilities to import external Home Affairs databases (CSV/SQL exports) via encrypted channels (TLS 1.2+).
*   **Validation Pipeline**: Multi-layer validation ensures only valid, consistent data enters the system (Type Check -> Luhn Check -> DOB Consistency).
*   **CRUD & Sync**: Supports batch upserts with transaction safety and optimistic concurrency control.
*   **Verification API**: Exposes endpoints to verify citizens against this local copy of the Home Affairs registry.
*   **Code**: [HomeAffairsImportService.cs](CrossSetaDeduplicator/src/CrossSetaWeb/Services/HomeAffairsImportService.cs)

### 🚦 Traffic Light System Automated Testing
A fully automated test suite (`src/CrossSetaWeb/scripts/test_traffic_light.js`) is included to verify the "Traffic Light" logic against the live API.

**To Run Tests:**
```bash
node src/CrossSetaWeb/scripts/test_traffic_light.js
```

**Test Data (Injected during Import):**
*   **Green (Alive)**: Valid ID + Matching Surname
*   **Red (Deceased)**: ID `0001010000001` (Injected Test Case) -> Returns DECEASED status.
*   **Yellow (Mismatch)**: Valid ID + Wrong Surname

### 5. 📑 Bulk Registration for Stakeholders
*   **Dedicated Portal**: A separate page allowing other SETAs, Companies, and Training Providers to register learners in bulk.
*   **CSV Upload**: Supports standard CSV format uploads for rapid data entry.
*   **Batch Processing**: Efficiently processes hundreds of records in a single transaction.
*   **Interactive UI**: Features real-time progress indicators (loading spinners) and professional branding (SeaGreen) to enhance user experience.
*   **Error Reporting**: Provides detailed feedback on successful registrations and any row-level errors.
*   **Code**: [BulkRegistrationService.cs](CrossSetaDeduplicator/src/CrossSetaWeb/Services/BulkRegistrationService.cs)

---

## 🌟 Innovation & Bonus Features

### 🛡️ Database Validation & Integrity
*   **Full Database Scan**: Administrators can trigger a system-wide audit that checks every existing learner against the Home Affairs registry.
*   **Smart Import Engine**: The system intelligently parses Home Affairs data files, automatically detecting Identity Numbers and Deceased status markers regardless of column order.
*   **Detailed Reporting**: Generates downloadable CSV reports highlighting:
    *   **Deceased Learners**: Flags individuals marked as deceased in the Home Affairs database.
    *   **Invalid IDs**: Detects ID numbers that fail the Luhn algorithm check.
    *   **Verification Discrepancies**: Identifies learners not found in the official registry.
*   **Code**: [DatabaseValidationService.cs](CrossSetaDeduplicator/src/CrossSetaWeb/Services/DatabaseValidationService.cs) and [HomeAffairsImportService.cs](CrossSetaDeduplicator/src/CrossSetaWeb/Services/HomeAffairsImportService.cs)

### 📶 Offline Capability
*   **Smart Queueing**: If the system detects network failure (or is toggled to "Simulate Offline"), verification requests are serialized and queued locally (`offline_verification_queue.json`).
*   **Secure Storage**: All sensitive information stored for offline purposes is **encrypted at rest** to ensure data protection in case of device theft or compromise.
*   **Automatic Processing**: The system is designed to process the queue automatically once connectivity is restored.

### 👤 Biometric & KYC Integration
*   **Document Scanning**: Integrates with **Doubango KYC SDK** (or simulation) to scan physical ID documents (Passport, ID Card).
*   **Facial Recognition**: Includes a biometric step to compare a live selfie against the photo in the scanned ID document.
*   **Code**: [KYCService.cs](CrossSetaDeduplicator/src/CrossSetaWeb/Services/KYCService.cs) and [RegistrationController.cs](CrossSetaDeduplicator/src/CrossSetaWeb/Controllers/RegistrationController.cs)

### 🧠 Intelligent Deduplication
*   **Fuzzy Matching**: Uses **Levenshtein Distance** algorithms to detect potential duplicates (e.g., "John Smith" vs "Jon Smith") alongside exact ID matching.
*   **Cross-SETA Ready**: Designed to check against a centralized database of learners.
*   **Code**: [DatabaseHelper.cs](CrossSetaDeduplicator/src/CrossSetaWeb/DataAccess/DatabaseHelper.cs)

---

## 🛠 Tech Stack & Architecture

*   **Desktop App**: VB.NET (.NET 6.0 / Windows Forms)
*   **Web Portal**: C# (ASP.NET Core MVC / .NET 10.0)
*   **Database**: SQL Server 2019 / Azure SQL Edge (Docker)
*   **Architecture**: N-Tier (UI Layer → Service Layer → Data Access Layer)
*   **Security**: Parameterized Queries (SQL Injection prevention), Environment Variable Configuration.

---

## 🚀 Installation & Setup

### 💻 1. Windows Installation (Native)
**This is the primary supported platform for CrossSetaDeduplicator.**

#### System Requirements
*   **OS**: Windows 10 (Version 1903+) or Windows 11.
*   **Architecture**: x64 processor.
*   **Framework**: .NET 6.0 Desktop Runtime (included with Visual Studio 2022).
*   **Database**: SQL Server 2019 Express or Developer Edition.

#### Step-by-Step Installation
1.  **Install Visual Studio 2022**:
    *   Download from [visualstudio.microsoft.com](https://visualstudio.microsoft.com/).
    *   During installation, select the workload: **".NET desktop development"**.
2.  **Install SQL Server**:
    *   Install SQL Server 2019 Express.
    *   Install **SQL Server Management Studio (SSMS)** for easy database management.
3.  **Clone the Repository**:
    ```powershell
    git clone https://github.com/aicofoundersite/IDverification
    cd IDverification
    ```
4.  **Database Configuration**:
    *   Open SSMS and connect to `.\SQLEXPRESS` (or `localhost`).
    *   Open `db/CreateDatabase.sql` and click **Execute**.
    *   Open `db/StoredProcedures.sql` and click **Execute**.
5.  **Run the Application**:
    *   Open `src/CrossSetaDeduplicator.sln` in Visual Studio.
    *   Press **F5** to build and run.

#### Verification
*   The **Main Dashboard** should launch without errors.
*   The "System Status" at the bottom should show "Connected".

---

### 🍎 2. macOS Installation (Virtualization)
**Note:** As a VB.NET Windows Forms application, this software **does not run natively on macOS**. You must use Windows virtualization.

#### System Requirements
*   **OS**: macOS Catalina (10.15) or newer.
*   **Hardware**: Intel or Apple Silicon (M1/M2/M3) Mac with at least 8GB RAM.
*   **Software**: Parallels Desktop, VMWare Fusion, or UTM (Free).

#### Step-by-Step Installation
1.  **Set up a Windows Virtual Machine**:
    *   **Apple Silicon (M1/M2/M3)**: Download and install **Windows 11 ARM64** using Parallels or UTM.
    *   **Intel Macs**: Install standard Windows 10/11 x64.
2.  **Configure the VM**:
    *   Ensure the VM has network access.
    *   Install **Visual Studio 2022 for Windows** (NOT Visual Studio for Mac) inside the VM.
3.  **Proceed with Windows Installation**:
    *   Once inside the Windows environment, follow the **Windows Installation** guide above exactly as written.

#### Troubleshooting Common macOS/VM Issues
*   **SQL Server on ARM (M1/M2)**: SQL Server 2019 does not run natively on Windows ARM.
    *   *Solution*: Use **Azure SQL Edge** (Docker) or connect to a remote SQL Server instance.
    *   *Workaround*: For this Hackathon prototype, the application can run in "Demo Mode" without a local DB if configured in `DatabaseHelper.vb`.

---

---

## 🧪 Testing the "Traffic Light" Logic

You can test the Real-Time Verification logic using the **Web Portal**, **Registration Wizard**, or the **Automated Test Script**.

### Automated Testing
We have included a dedicated Node.js script to automatically verify the traffic light logic against the Home Affairs database.

1.  **Run the Test Script**:
    ```bash
    node src/CrossSetaWeb/scripts/test_traffic_light.js
    ```
2.  **Expected Output**:
    The script will run through predefined test cases (Green, Yellow, Red) and output the results to the console.

### Manual Testing Scenarios

| Scenario | Input ID | Input Name | Input Surname | Expected Result |
| :--- | :--- | :--- | :--- | :--- |
| **Valid Citizen** | `0002080806082` | Sichumile | Makaula | 🟢 **Green** (Verified) |
| **Surname Mismatch** | `0002080806082` | Sichumile | *Smith* | 🟡 **Yellow** (Mismatch Warning) |
| **Deceased** | `0001010000001` | Any | Any | 🔴 **Red** (Deceased Alert) |
| **Not Found** | `9999999999999` | Any | Any | 🔴 **Red** (Identity Not Found) |
| **Invalid Format** | `12345` | Any | Any | 🔴 **Red** (Invalid Format) |

*Note: The IDs above are part of the demo dataset imported from the secure Google Sheet.*

---

## 📂 Project Structure

*   `src/` - Source code (VB.NET)
    *   `Forms/` - UI Windows Forms (RegistrationWizard, MainDashboard)
    *   `Services/` - Business Logic (HomeAffairsService, KYCService, DeduplicationService)
    *   `DataAccess/` - Database interactions (DatabaseHelper)
    *   `Models/` - Data objects
*   `db/` - SQL Scripts for database creation and management.
*   `lib/` - External libraries (KYC SDKs).

