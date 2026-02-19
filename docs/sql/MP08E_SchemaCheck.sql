PRINT '=== MP08E Schema Sanity Check ===';

SELECT 'cm.ChangeRequest' AS [Object], IIF(OBJECT_ID('cm.ChangeRequest','U') IS NOT NULL, 'OK', 'MISSING') AS [Status]
UNION ALL SELECT 'cm.ChangeApprover', IIF(OBJECT_ID('cm.ChangeApprover','U') IS NOT NULL, 'OK', 'MISSING')
UNION ALL SELECT 'cm.ChangeApproval', IIF(OBJECT_ID('cm.ChangeApproval','U') IS NOT NULL, 'OK', 'MISSING')
UNION ALL SELECT 'cm.ChangeAttachment', IIF(OBJECT_ID('cm.ChangeAttachment','U') IS NOT NULL, 'OK', 'MISSING');

SELECT 'cm.ChangeRequest.RequestedByUserId' AS [Column], IIF(COL_LENGTH('cm.ChangeRequest','RequestedByUserId') IS NOT NULL, 'OK', 'MISSING') AS [Status]
UNION ALL SELECT 'cm.ChangeRequest.AssignedToUserId', IIF(COL_LENGTH('cm.ChangeRequest','AssignedToUserId') IS NOT NULL, 'OK', 'MISSING')
UNION ALL SELECT 'cm.ChangeRequest.ApprovalRequesterUserId', IIF(COL_LENGTH('cm.ChangeRequest','ApprovalRequesterUserId') IS NOT NULL, 'OK', 'MISSING')
UNION ALL SELECT 'cm.ChangeRequest.ApprovalRequired', IIF(COL_LENGTH('cm.ChangeRequest','ApprovalRequired') IS NOT NULL, 'OK', 'MISSING')
UNION ALL SELECT 'cm.ChangeRequest.ApprovalStrategy', IIF(COL_LENGTH('cm.ChangeRequest','ApprovalStrategy') IS NOT NULL, 'OK', 'MISSING')
UNION ALL SELECT 'cm.ChangeRequest.ImplementationGroup', IIF(COL_LENGTH('cm.ChangeRequest','ImplementationGroup') IS NOT NULL, 'OK', 'MISSING')
UNION ALL SELECT 'cm.ChangeRequest.SubmittedAt', IIF(COL_LENGTH('cm.ChangeRequest','SubmittedAt') IS NOT NULL, 'OK', 'MISSING')
UNION ALL SELECT 'cm.ChangeRequest.SubmittedByUserId', IIF(COL_LENGTH('cm.ChangeRequest','SubmittedByUserId') IS NOT NULL, 'OK', 'MISSING')
UNION ALL SELECT 'cm.ChangeRequest.DeletedAt', IIF(COL_LENGTH('cm.ChangeRequest','DeletedAt') IS NOT NULL, 'OK', 'MISSING')
UNION ALL SELECT 'cm.ChangeRequest.DeletedByUserId', IIF(COL_LENGTH('cm.ChangeRequest','DeletedByUserId') IS NOT NULL, 'OK', 'MISSING')
UNION ALL SELECT 'cm.ChangeRequest.DeletedReason', IIF(COL_LENGTH('cm.ChangeRequest','DeletedReason') IS NOT NULL, 'OK', 'MISSING');

SELECT 'cm.ChangeAttachment.FileUrl' AS [Column], IIF(COL_LENGTH('cm.ChangeAttachment','FileUrl') IS NOT NULL, 'OK', 'MISSING') AS [Status]
UNION ALL SELECT 'cm.ChangeAttachment.FilePath', IIF(COL_LENGTH('cm.ChangeAttachment','FilePath') IS NOT NULL, 'OK', 'MISSING')
UNION ALL SELECT 'cm.ChangeAttachment.FileName', IIF(COL_LENGTH('cm.ChangeAttachment','FileName') IS NOT NULL, 'OK', 'MISSING')
UNION ALL SELECT 'cm.ChangeAttachment.FileSizeBytes', IIF(COL_LENGTH('cm.ChangeAttachment','FileSizeBytes') IS NOT NULL, 'OK', 'MISSING')
UNION ALL SELECT 'cm.ChangeAttachment.UploadedAt', IIF(COL_LENGTH('cm.ChangeAttachment','UploadedAt') IS NOT NULL, 'OK', 'MISSING')
UNION ALL SELECT 'cm.ChangeAttachment.UploadedBy', IIF(COL_LENGTH('cm.ChangeAttachment','UploadedBy') IS NOT NULL, 'OK', 'MISSING');

SELECT 'IX_ChangeApprover_ChangeRequestId_ApproverUserId' AS [IndexName],
       IIF(EXISTS(SELECT 1 FROM sys.indexes WHERE name='IX_ChangeApprover_ChangeRequestId_ApproverUserId' AND object_id=OBJECT_ID('cm.ChangeApprover')),'OK','MISSING') AS [Status];
