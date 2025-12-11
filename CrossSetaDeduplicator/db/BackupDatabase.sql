-- Backup Script for CrossSetaDB
-- Usage: Run this script in SSMS or via sqlcmd to create a full backup.
-- Note: Ensure the backup directory exists and the SQL Server service account has write permissions.

DECLARE @BackupFile NVARCHAR(255);
SET @BackupFile = 'C:\Backups\CrossSetaDB_' + FORMAT(GETDATE(), 'yyyyMMdd_HHmmss') + '.bak';

BACKUP DATABASE CrossSetaDB
TO DISK = @BackupFile
WITH FORMAT,
     MEDIANAME = 'CrossSetaDB_Backup',
     NAME = 'Full Backup of CrossSetaDB';

PRINT 'Backup completed successfully to ' + @BackupFile;
GO
