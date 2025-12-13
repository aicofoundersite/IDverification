# CrossSetaDeduplicator - Intelligent ID Verification & Deduplication System

## 📋 Project Overview
**CrossSetaDeduplicator** is a comprehensive identity verification solution designed to eliminate duplicate learner registrations across multiple Sector Education and Training Authorities (SETAs). By leveraging real-time validation against Department of Home Affairs data and implementing advanced fuzzy matching algorithms, the system ensures data integrity and prevents fraudulent or erroneous "double-dipping" of grant funding.

This solution is built on a strict **N-Tier Architecture**, utilizing **ASP.NET Core** for the web interface and **Visual Basic .NET (VB.NET)** for core business logic, backed by **SQL Server** for robust data management.

### ☁️ Cloud Deployment
The application is fully containerized using **Docker** and deployed to **Fly.io** for high availability and global scalability.

*   **Live Web Portal**: [https://cross-seta-web-17655.fly.dev/](https://cross-seta-web-17655.fly.dev/)
*   **Documentation**: [https://cross-seta-web-17655.fly.dev/Home/Documentation](https://cross-seta-web-17655.fly.dev/Home/Documentation)

---

## 🎯 Core Capabilities

### 1. 🔍 Deduplication Engine
*   **Real-time Check**: Validates ID numbers against the central database during registration to prevent duplicate entries across SETAs.
*   **Conflict Resolution**: Flags potential duplicates for manual review.
*   **VB.NET Logic**: Core deduplication algorithms are implemented in a high-performance VB.NET class library (`CrossSetaLogic`).

### 2. ⚡ Home Affairs Validation (Traffic Light Protocol)
Connects to a simulated Department of Home Affairs database to verify identity status in real-time.
*   🟢 **GREEN (Verified)**: ID exists, is "Alive", and the Surname matches the official record.
*   🟡 **YELLOW (Warning)**: ID is valid, but the Surname provided does not match the official record (potential marriage/typo).
*   🔴 **RED (Invalid/Fraud)**: ID does not exist, or the person is marked as "Deceased".

### 3. 📑 Bulk Processing
*   **CSV Import**: Supports standardized CSV templates for uploading large datasets of learner data.
*   **Async Validation**: Processes records in the background with progress tracking.
*   **Detailed Reporting**: Generates reports on successful imports and validation failures.

### 4. 🔒 Security & Compliance
*   **Supabase Auth**: Secure user management and JWT-based authentication.
*   **Audit Trail**: Full traceability of every verification attempt, logging User, Timestamp, Source, and Result.
*   **POPIA Compliance**: Mandatory consent checks before processing personal information.

---

## 🛠️ Technical Architecture

| Component | Technology | Description |
| :--- | :--- | :--- |
| **Frontend** | ASP.NET Core MVC | Responsive web interface with Bootstrap 5. |
| **Core Logic** | Visual Basic .NET | Business rules and validation logic in `CrossSetaLogic`. |
| **Database** | SQL Server | Relational data storage. |
| **Auth** | Supabase | Identity and Access Management. |
| **Deployment** | Fly.io (Docker) | Containerized cloud hosting. |

---

## 🚀 Getting Started

### Prerequisites
*   .NET 8.0 SDK or later
*   SQL Server
*   Supabase Account

### Installation
```bash
git clone https://github.com/your-repo/IDverification.git
cd CrossSetaDeduplicator
dotnet restore
dotnet run --project src/CrossSetaWeb/CrossSetaWeb.csproj
```

### API Reference
The system exposes REST endpoints for integration:
*   `GET /api/Verification/{id}` - Check local database.
*   `GET /api/Verification/home-affairs/{id}` - Verify against Home Affairs.
*   `POST /api/Import/trigger` - Trigger bulk import.
