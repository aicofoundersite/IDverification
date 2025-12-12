-- Table: ExternalVerificationLog
-- Tracks every attempt to verify an ID against an external source (e.g., Home Affairs).
CREATE TABLE ExternalVerificationLog (
    LogID INT IDENTITY(1,1) PRIMARY KEY,
    NationalID NVARCHAR(20) NOT NULL, -- The ID being verified
    VerificationSource NVARCHAR(50), -- 'HomeAffairs', 'KYC_SDK', 'OfflineQueue'
    Status NVARCHAR(50), -- 'Success', 'Failed', 'NotFound', 'Mismatch', 'OfflineQueued'
    Details NVARCHAR(MAX), -- JSON or Text details of the result/error
    RequestTimestamp DATETIME DEFAULT GETDATE(),
    ResponseTimestamp DATETIME, -- To track latency
    PerformedBy NVARCHAR(100) DEFAULT 'System' -- User who initiated check
);
GO

-- Index for reporting
CREATE INDEX IX_VerificationLog_NationalID ON ExternalVerificationLog(NationalID);
CREATE INDEX IX_VerificationLog_Date ON ExternalVerificationLog(RequestTimestamp);
GO
