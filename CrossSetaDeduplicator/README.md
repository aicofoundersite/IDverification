# Cross-SETA Deduplication & ID Verification System

A robust, enterprise-grade web application designed to eliminate duplicate learner registrations across Sector Education and Training Authorities (SETAs). This solution features real-time validation, bulk processing, and seamless integration with simulated Home Affairs data to prevent "double-dipping" and ensure data integrity.

## 🚀 Live Deployment
**URL:** [https://cross-seta-web-17655.fly.dev/](https://cross-seta-web-17655.fly.dev/)

*   **Login**: Use the "Register" page to create an account, or log in with existing credentials.
*   **Documentation**: [https://cross-seta-web-17655.fly.dev/Home/Documentation](https://cross-seta-web-17655.fly.dev/Home/Documentation)

---

## 🧪 Testing Scenarios & Demo IDs (Critical for Hackathon Judges)

To fully experience the **"Traffic Light" Verification Logic**, use the following specific National ID numbers during testing (either in "Register Learner" or "Bulk Import").

### 🟢 Scenario 1: Valid Citizen (Green Status)
*   **National ID**: `0002080806082`
*   **Input Surname**: `Makaula`
*   **Expected Result**: **Verification Successful (Green)**
*   **Description**: This represents a perfect match. The ID exists in Home Affairs, the citizen is Alive, and the provided surname matches the official record.

### 🟡 Scenario 2: Surname Mismatch (Yellow Status)
*   **National ID**: `0002080806082`
*   **Input Surname**: `Smith` (or any name other than "Makaula")
*   **Expected Result**: **Flagged for Review (Yellow)**
*   **Description**: The ID is valid and the citizen is Alive, but the surname provided does not match the official Home Affairs record (e.g., maiden name vs married name, or a typo). The system flags this for manual review rather than blocking it outright.

### 🔴 Scenario 3: Deceased Citizen (Red Status)
*   **National ID**: `0001010000001`
*   **Input Surname**: *(Any)*
*   **Expected Result**: **Blocked - Deceased (Red)**
*   **Description**: The ID exists in Home Affairs, but the citizen is marked as **Deceased**. Registration is immediately blocked to prevent fraud.

### 🔴 Scenario 4: ID Not Found (Red Status)
*   **National ID**: `9999999999999`
*   **Input Surname**: *(Any)*
*   **Expected Result**: **Blocked - ID Not Found (Red)**
*   **Description**: The ID number does not exist in the Home Affairs database. This suggests a fake or invalid identity.

---

## ✅ Core Requirements (The "Must-Haves")

### 1. Input Validation
*   **South African ID Validation**: Implements the **Luhn Algorithm** to mathematically validate the ID number structure before any database checks.
    *   **Code Reference**: [HomeAffairsService.vb - ValidateIDFormat](src/CrossSetaWeb/CrossSetaLogic/Services/HomeAffairsService.vb)
*   **Data Sanitization**: Regex-based validation for Name and Surname formats ensures high-quality data entry.

### 2. Real-Time Verification
*   **Home Affairs Simulation**: Connects to a simulated external data source (Department of Home Affairs) to verify citizen identity, "Alive/Deceased" status, and surname consistency in real-time.
    *   **Code Reference**: [VerificationController.cs - VerifyHomeAffairs](src/CrossSetaWeb/Controllers/Api/VerificationController.cs)

### 3. Audit Trail
*   **Full Accountability**: The system logs **every single verification attempt**.
*   **Captured Data**: User performing the action, Timestamp, Input Data (masked), Result Status, and Error Messages.
    *   **Code Reference**: [DatabaseHelper.vb - LogExternalVerification](src/CrossSetaWeb/CrossSetaLogic/DataAccess/DatabaseHelper.vb)

---

## 🌟 Bonus Points for Innovation

### 1. Offline Capability
*   **Resilience**: The system features an "Offline Mode" simulation. When the external Home Affairs service is unreachable, requests are queued locally and automatically retried once connectivity is restored.
    *   **Code Reference**: [HomeAffairsService.vb - VerifyCitizenAsync](src/CrossSetaWeb/CrossSetaLogic/Services/HomeAffairsService.vb)

### 2. User Experience (UX) - "Traffic Light" Logic
*   **Visual Feedback**: Distinct visual cues allow operators to instantly understand verification status.
    *   🟢 **Green**: Verified & Clear.
    *   🟡 **Yellow**: Requires Attention (Mismatch).
    *   🔴 **Red**: Critical Stop (Fraud/Deceased).
*   **Clarity**: Error messages are human-readable and actionable.

### 3. Security & Compliance
*   **POPIA Compliance**: Personally Identifiable Information (PII) is masked in all administrative views to protect user privacy.
*   **Secure Configuration**: No hardcoded API keys or credentials. All sensitive configuration is managed via Environment Variables.

---

## 🏗 Architecture Expectations

The solution is built on a **Modular N-Tier Architecture** to ensure maintainability, scalability, and separation of concerns:

1.  **Presentation Layer (Frontend)**:
    *   **Technology**: ASP.NET Core MVC (C#).
    *   **Role**: Handles user interaction, UI rendering (Razor Views), and acts as the API gateway.
2.  **Business Logic Layer (BLL)**:
    *   **Technology**: VB.NET Class Library (`CrossSetaLogic`).
    *   **Role**: Encapsulates all core business rules, validation logic, and the "Traffic Light" protocol. This demonstrates multi-language interoperability within .NET.
3.  **Data Access Layer (DAL)**:
    *   **Technology**: SQL Server (ADO.NET).
    *   **Role**: Manages efficient data persistence using **Parameterized Queries** to prevent SQL injection.

---

## 🔐 Advanced Security & Verification

### 1. KYC Verification (Know Your Customer)
To comply with FICA and SETA regulations, the system captures and validates comprehensive personal information beyond just the ID number:
*   **Data Points**: Address, Contact Details, Socio-Economic Status, Disability Status.
*   **Code Reference**: [DatabaseHelper.vb - InsertLearner](https://github.com/aicofoundersite/IDverification/blob/main/CrossSetaDeduplicator/src/CrossSetaWeb/CrossSetaLogic/DataAccess/DatabaseHelper.vb)

### 2. Biometric Capabilities
The system is ready for future biometric integration:
*   **Biometric Hashing**: The database schema and application logic include a `BiometricHash` field. This allows for the storage of fingerprint or facial recognition templates as secure hashes, enabling multi-factor authentication.
*   **Code Reference**:
    *   [LearnerModel.vb - BiometricHash Property](https://github.com/aicofoundersite/IDverification/blob/main/CrossSetaDeduplicator/src/CrossSetaWeb/CrossSetaLogic/Models/LearnerModel.vb)
    *   [DatabaseHelper.vb - Database Persistence](https://github.com/aicofoundersite/IDverification/blob/main/CrossSetaDeduplicator/src/CrossSetaWeb/CrossSetaLogic/DataAccess/DatabaseHelper.vb)

---

## 📋 Mandatory Features (BRD Alignment)

### 1. Pre-Registration Check
Before any learner is permanently registered, the system performs a cross-check against the local W&RSETA database. If the learner is already registered (possibly under a different training provider), the system flags it immediately to prevent duplicate funding applications.

### 2. Duplicate Attempt Reporting
The system generates detailed reports of duplicate attempts, allowing administrators to identify patterns of attempted fraud or administrative error.

---

## 🛠 Tech Stack

*   **Frontend**: ASP.NET Core MVC (Razor Views), Bootstrap 5
*   **Backend (Web)**: C# .NET 8.0
*   **Backend (Logic)**: VB.NET .NET 8.0
*   **Database**: Microsoft SQL Server (Linux Container)
*   **Deployment**: Docker, Fly.io

---

## 💻 Getting Started (Local)

1.  **Clone the Repository**:
    ```bash
    git clone https://github.com/aicofoundersite/IDverification.git
    cd IDverification/CrossSetaDeduplicator
    ```

2.  **Run the Application**:
    ```bash
    cd src/CrossSetaWeb
    dotnet run
    ```
    Access the site at `http://localhost:5000`.

---

## 📞 Contact

**Team MLX Ventures**
*   *Project for ID Verification Hackathon*
