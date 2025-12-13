# Home Affairs Database Import - Data Mapping & Strategy

## 1. Overview
This document outlines the strategy for importing the Home Affairs Database into the CrossSetaDeduplicator application, replacing the mock simulation.

**Source**: SQL Backup / CSV Export (Simulated for Implementation)
**Target**: SQL Server `HomeAffairsCitizens` Table

## 2. Field Mapping

| Source Field (CSV/DB) | Target Column (SQL) | Data Type | Validation Rules | Sanitization |
|-----------------------|---------------------|-----------|------------------|--------------|
| Identity Number       | NationalID          | NVARCHAR(13) | Required, Numeric, Length=13, Luhn Check | Trim, Whitelist(0-9) |
| First Name            | FirstName           | NVARCHAR(100)| Required, MaxLen=100 | Trim, HTML Encode, Title Case |
| Surname               | Surname             | NVARCHAR(100)| Required, MaxLen=100 | Trim, HTML Encode, Title Case |
| Date of Birth         | DateOfBirth         | DATE      | Required, Past Date, Matches ID DOB segment | Date Parsing |
| Deceased Status       | IsDeceased          | BIT       | Boolean | Default=0 |

## 3. Security & Validation Layers

### 3.1 Transport Security
- **TLS 1.2+**: All data fetching from external sources (Google Drive/Sheets) is performed via HTTPS using `HttpClient` with forced TLS 1.2+ protocols.

### 3.2 Data Validation (Multi-Layer)
1.  **Type Checking**: Ensure Dates are valid DateTime objects, Strings are within length limits.
2.  **Format Verification**:
    - **NationalID**: Must pass the Luhn Algorithm (Modulus 10).
    - **Date of Birth**: Must match the first 6 digits of the ID (YYMMDD).
3.  **Sanitization (OWASP)**:
    - Input stripping of script tags/HTML.
    - Parameterized SQL queries to prevent Injection.

### 3.3 Integrity
- **Referential Integrity**: NationalID is the Primary Key.
- **Concurrency**: `RowVersion` (Timestamp) column used for Optimistic Concurrency Control.

## 4. Import Process (CRUD & Sync)

1.  **Batch Processing**: Records are processed in batches of 1000 to manage memory.
2.  **Transaction Support**: Each batch is committed within a `SqlTransaction`. If any record in a batch fails, the batch is rolled back (or erroneous records logged to an error table).
3.  **Bi-directional Sync Strategy**:
    - **Change Data Capture (CDC)**: Timestamps (`LastUpdated`) track changes.
    - **Conflict Resolution**: "Last Writer Wins" policy based on `LastUpdated`.

## 5. Production Readiness

- **RBAC**: Only users with `Admin` role can trigger imports.
- **Audit**: All import actions are logged to `SystemAuditLog`.
- **Monitoring**: Metrics on "Records Processed", "Failed Rows", and "Duration" are logged.
