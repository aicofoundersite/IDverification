USE CrossSetaDB;
GO

-- 1. Optimize DuplicateAuditLog for Reporting
-- These indexes help in generating reports by date or looking up history for a specific ID.

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_DuplicateAuditLog_CheckDate' AND object_id = OBJECT_ID('DuplicateAuditLog'))
BEGIN
    CREATE INDEX IX_DuplicateAuditLog_CheckDate ON DuplicateAuditLog(CheckDate);
    PRINT 'Index IX_DuplicateAuditLog_CheckDate created.';
END
GO

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_DuplicateAuditLog_TestedNationalID' AND object_id = OBJECT_ID('DuplicateAuditLog'))
BEGIN
    CREATE INDEX IX_DuplicateAuditLog_TestedNationalID ON DuplicateAuditLog(TestedNationalID);
    PRINT 'Index IX_DuplicateAuditLog_TestedNationalID created.';
END
GO

-- 2. Verify Learners Indexing
-- Ensure we have coverage for the Role column if we filter by it often (e.g., "Select all Assessors")
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_Learners_Role' AND object_id = OBJECT_ID('Learners'))
BEGIN
    CREATE INDEX IX_Learners_Role ON Learners(Role);
    PRINT 'Index IX_Learners_Role created.';
END
GO

PRINT 'Database optimization completed.';
