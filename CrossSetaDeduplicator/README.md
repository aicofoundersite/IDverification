# Cross-SETA Deduplication & ID Verification System

A robust, enterprise-grade web application designed to eliminate duplicate learner registrations across Sector Education and Training Authorities (SETAs). This solution features real-time validation, bulk processing, and seamless integration with simulated Home Affairs data.

## 🚀 Live Deployment
**URL:** [https://cross-seta-web-17655.fly.dev/](https://cross-seta-web-17655.fly.dev/)

*   **Login**: Use the "Register" page to create an account, or log in with existing credentials.
*   **Documentation**: [https://cross-seta-web-17655.fly.dev/Home/Documentation](https://cross-seta-web-17655.fly.dev/Home/Documentation)

---

## ✅ Core Requirements (The "Must-Haves")

1.  **Input Validation**:
    *   Strict validation of South African ID numbers using the **Luhn Algorithm**.
    *   Regex-based validation for Name and Surname formats to ensure data quality.
2.  **Real-Time Verification**:
    *   Instant connection to an external data source (Simulated Department of Home Affairs) to verify citizen identity and status.
3.  **Audit Trail**:
    *   Logs **every single verification attempt**, capturing the User, Timestamp, Input Data, and Result Status for full accountability.

---

## 🌟 Bonus Points for Innovation

1.  **Offline Capability**:
    *   Implements queueing and retry logic to handle verifications during external system downtimes.
2.  **User Experience (UX)**:
    *   **Traffic Light Logic**: Uses distinct visual cues (Green/Yellow/Red badges) to communicate verification status instantly.
    *   Clear, human-readable error messages and input masking.
3.  **Security & Compliance**:
    *   **POPIA Compliance**: PII (Personally Identifiable Information) is masked in administrative views.
    *   No hardcoded API keys or credentials in source code.

---

## 🏗 Architecture Expectations

*   **Modular N-Tier Structure**:
    *   **Frontend**: ASP.NET Core MVC (C#) for the presentation layer.
    *   **Logic**: VB.NET Class Library (`CrossSetaLogic`) for business rules and validation.
    *   **Data**: SQL Server with parameterized queries for secure data access.

---

## 🚦 Verification Logic: "Traffic Light" Protocol

The system applies an **Interceptor Pattern** to categorize verification results:

*   🟢 **Green (Verified)**: Identity confirmed, Name/Surname match, Citizen is Alive.
*   🟡 **Yellow (Review)**: Identity confirmed, but Surname mismatch or minor discrepancy found.
*   🔴 **Red (Invalid/Fraud)**: ID not found, Citizen is Deceased, or Invalid ID format.

---

## 🧪 Testing Scenarios & Demo IDs

Use the following IDs to test the specific "Traffic Light" scenarios in the system:

| Scenario | Status | National ID | Surname Input | Expected Result |
| :--- | :--- | :--- | :--- | :--- |
| **Valid Citizen** | 🟢 Green | `0002080806082` | `Makaula` | Verification Successful. |
| **Surname Mismatch** | 🟡 Yellow | `0002080806082` | `Smith` | Flagged for Review. |
| **Deceased Citizen** | 🔴 Red | `0001010000001` | *(Any)* | Blocked (Deceased). |
| **ID Not Found** | 🔴 Red | `9999999999999` | *(Any)* | Blocked (Not Found). |

---

## 📋 Mandatory Features (BRD Alignment)

1.  **Pre-Registration Check**:
    *   Checks for existing learner records in the local W&RSETA database before committing new registrations.
2.  **Duplicate Attempt Reporting**:
    *   Generates detailed reports of duplicate attempts to prevent "double-dipping".

---

## 📊 Reporting Requirements

*   **Administrative Dashboard**:
    *   Visualizes verification statistics (Success vs. Failure).
    *   Exports lists of "Blocked" or "Flagged" attempts for audit purposes.

---

## 🛠 Tech Stack

*   **Frontend**: ASP.NET Core MVC (Razor Views), Bootstrap 5
*   **Backend (Web)**: C# .NET 8.0
*   **Backend (Logic)**: VB.NET .NET 8.0 (Demonstrating multi-language solution)
*   **Database**: Microsoft SQL Server (Linux Container)
*   **Deployment**: Docker, Fly.io

---

## 💻 Getting Started (Local)

1.  **Clone the Repository**:
    ```bash
    git clone https://github.com/YourUsername/IDverification.git
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
