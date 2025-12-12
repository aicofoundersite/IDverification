# CrossSetaDeduplicator - Intelligent ID Verification & Deduplication System

## 📋 Project Overview
**CrossSetaDeduplicator** is a production-ready **VB.NET Windows Forms** application designed to solve the critical challenge of identity verification for W&RSETA. It provides a secure, real-time mechanism to validate South African ID numbers, prevent duplicate learner registrations, and ensure data integrity across the SETA landscape.

This solution is built on a strict **N-Tier Architecture**, utilizing **SQL Server 2019** for robust data management and audit compliance.

---

## 🎯 Core Capabilities (The "Must-Haves")

### 1. ✅ Input Validation
*   **South African ID Standard**: Implements the **Luhn Algorithm (Modulus 10)** to mathematically validate ID numbers before they leave the client.
*   **Format Enforcement**: Strict input masking ensures 13-digit numeric compliance.
*   **Data Integrity**: Mandatory checks for Name and Surname to ensure complete records.

### 2. ⚡ Real-Time Verification (Traffic Light Protocol)
Connects to an external trusted data source (simulating Department of Home Affairs) to verify identity status in real-time.
*   🟢 **GREEN (Verified)**: ID exists, is "Alive", and the Surname matches the official record.
*   🟡 **YELLOW (Warning)**: ID is valid, but the Surname provided does not match the official record (potential marriage/typo).
*   🔴 **RED (Invalid/Fraud)**: ID does not exist, or the person is marked as "Deceased".

### 3. 🔒 Audit Trail & Compliance
*   **Full Traceability**: Every single verification attempt is logged in the `ExternalVerificationLog` table.
*   **Who, When, What**: Logs the **Timestamp**, **User** (System User), **Source** (e.g., HomeAffairs, KYC_SDK), and **Result Status**.
*   **POPIA Compliance**: Mandatory "Consent" checkbox explicitly required before any verification can proceed.

---

## 🌟 Innovation & Bonus Features

### 📶 Offline Capability
*   **Smart Queueing**: If the system detects network failure (or is toggled to "Simulate Offline"), verification requests are serialized and queued locally (`offline_verification_queue.json`).
*   **Automatic Processing**: The system is designed to process the queue automatically once connectivity is restored.

### 👤 Biometric & KYC Integration
*   **Document Scanning**: Integrates with **Doubango KYC SDK** (or simulation) to scan physical ID documents (Passport, ID Card).
*   **Facial Recognition**: Includes a biometric step to compare a live selfie against the photo in the scanned ID document.

### 🧠 Intelligent Deduplication
*   **Fuzzy Matching**: Uses **Levenshtein Distance** algorithms to detect potential duplicates (e.g., "John Smith" vs "Jon Smith") alongside exact ID matching.
*   **Cross-SETA Ready**: Designed to check against a centralized database of learners.

---

## 🛠 Tech Stack & Architecture

*   **Language**: VB.NET (.NET 6.0 / Framework 4.8 compatible)
*   **Database**: SQL Server 2019
*   **Architecture**: N-Tier (UI Layer → Service Layer → Data Access Layer)
*   **Security**: Parameterized Queries (SQL Injection prevention), Environment Variable Configuration.

---

## 🚀 Installation & Setup

### Prerequisites
*   Windows 10/11
*   Visual Studio 2022
*   SQL Server 2019 (Express or Developer)

### Database Setup
1.  Open **SQL Server Management Studio (SSMS)**.
2.  Execute `db/CreateDatabase.sql` to build the schema.
3.  Execute `db/StoredProcedures.sql` to install the required logic.
4.  *(Optional)* Execute `db/UpdateDatabase_VerificationLog.sql` if upgrading an older version.

### Running the Application
1.  Open `src/CrossSetaDeduplicator.sln` in Visual Studio.
2.  Check `DatabaseHelper.vb` or set the `CROSS_SETA_DB_CONNECTION` environment variable to point to your SQL instance.
3.  Run the application (**F5**).
4.  **Credentials**: No login required for the prototype (Windows Auth).

---

## 🧪 Testing the "Traffic Light" Logic

You can test the Real-Time Verification logic using the **Registration Wizard**:

| Scenario | Input ID | Input Name | Input Surname | Expected Result |
| :--- | :--- | :--- | :--- | :--- |
| **Valid Citizen** | `9505055000081` | Thabo | Molefe | 🟢 **Green** (Verified) |
| **Surname Mismatch** | `9505055000081` | Thabo | *Unknown* | 🟡 **Yellow** (Mismatch Warning) |
| **Deceased** | `9001015000085` | Any | Any | 🔴 **Red** (Deceased Alert) |
| **Invalid Format** | `12345` | Any | Any | 🔴 **Red** (Invalid Format) |

---

## 📂 Project Structure

*   `src/` - Source code (VB.NET)
    *   `Forms/` - UI Windows Forms (RegistrationWizard, MainDashboard)
    *   `Services/` - Business Logic (HomeAffairsService, KYCService, DeduplicationService)
    *   `DataAccess/` - Database interactions (DatabaseHelper)
    *   `Models/` - Data objects
*   `db/` - SQL Scripts for database creation and management.
*   `lib/` - External libraries (KYC SDKs).

