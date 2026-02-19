using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ChangeManagement.Api.Migrations;

[Migration("20260221000000_MP08E_DbReconcile_WorkflowTables")]
public partial class MP08EDbReconcileWorkflowTables : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql(@"
IF SCHEMA_ID('cm') IS NULL EXEC('CREATE SCHEMA [cm]');

IF OBJECT_ID('[cm].[ChangeApprover]', 'U') IS NULL
BEGIN
    CREATE TABLE [cm].[ChangeApprover]
    (
        [ChangeApproverId] UNIQUEIDENTIFIER NOT NULL,
        [ChangeRequestId] UNIQUEIDENTIFIER NOT NULL,
        [ApproverUserId] UNIQUEIDENTIFIER NOT NULL,
        [CreatedAt] DATETIME2 NOT NULL CONSTRAINT [DF_ChangeApprover_CreatedAt] DEFAULT(GETUTCDATE()),
        CONSTRAINT [PK_ChangeApprover] PRIMARY KEY ([ChangeApproverId])
    );
END

IF OBJECT_ID('[cm].[ChangeApproval]', 'U') IS NULL
BEGIN
    CREATE TABLE [cm].[ChangeApproval]
    (
        [ChangeApprovalId] UNIQUEIDENTIFIER NOT NULL,
        [ChangeRequestId] UNIQUEIDENTIFIER NOT NULL,
        [ApproverUserId] UNIQUEIDENTIFIER NOT NULL,
        [ApprovalStatusId] INT NOT NULL,
        [ApprovedAt] DATETIME2 NULL,
        [Comments] NVARCHAR(1000) NULL,
        CONSTRAINT [PK_ChangeApproval] PRIMARY KEY ([ChangeApprovalId])
    );
END

IF OBJECT_ID('[cm].[ChangeAttachment]', 'U') IS NOT NULL
BEGIN
    IF COL_LENGTH('cm.ChangeAttachment', 'FileName') IS NULL ALTER TABLE [cm].[ChangeAttachment] ADD [FileName] NVARCHAR(255) NOT NULL CONSTRAINT [DF_ChangeAttachment_FileName] DEFAULT('');
    IF COL_LENGTH('cm.ChangeAttachment', 'FileUrl') IS NULL ALTER TABLE [cm].[ChangeAttachment] ADD [FileUrl] NVARCHAR(500) NOT NULL CONSTRAINT [DF_ChangeAttachment_FileUrl] DEFAULT('');
    IF COL_LENGTH('cm.ChangeAttachment', 'FilePath') IS NULL ALTER TABLE [cm].[ChangeAttachment] ADD [FilePath] NVARCHAR(500) NOT NULL CONSTRAINT [DF_ChangeAttachment_FilePath] DEFAULT('');
    IF COL_LENGTH('cm.ChangeAttachment', 'FileSizeBytes') IS NULL ALTER TABLE [cm].[ChangeAttachment] ADD [FileSizeBytes] BIGINT NOT NULL CONSTRAINT [DF_ChangeAttachment_FileSizeBytes] DEFAULT(0);
    IF COL_LENGTH('cm.ChangeAttachment', 'UploadedAt') IS NULL ALTER TABLE [cm].[ChangeAttachment] ADD [UploadedAt] DATETIME2 NOT NULL CONSTRAINT [DF_ChangeAttachment_UploadedAt] DEFAULT(GETUTCDATE());
    IF COL_LENGTH('cm.ChangeAttachment', 'UploadedBy') IS NULL ALTER TABLE [cm].[ChangeAttachment] ADD [UploadedBy] UNIQUEIDENTIFIER NULL;
    UPDATE [cm].[ChangeAttachment] SET [FilePath] = [FileUrl] WHERE ISNULL([FilePath], '') = '';
END

IF OBJECT_ID('[cm].[ChangeRequest]', 'U') IS NOT NULL
BEGIN
    IF COL_LENGTH('cm.ChangeRequest', 'RequestedByUserId') IS NULL ALTER TABLE [cm].[ChangeRequest] ADD [RequestedByUserId] UNIQUEIDENTIFIER NOT NULL CONSTRAINT [DF_ChangeRequest_RequestedByUserId] DEFAULT('11111111-1111-1111-1111-111111111111');
    IF COL_LENGTH('cm.ChangeRequest', 'AssignedToUserId') IS NULL ALTER TABLE [cm].[ChangeRequest] ADD [AssignedToUserId] UNIQUEIDENTIFIER NULL;
    IF COL_LENGTH('cm.ChangeRequest', 'ApprovalRequesterUserId') IS NULL ALTER TABLE [cm].[ChangeRequest] ADD [ApprovalRequesterUserId] UNIQUEIDENTIFIER NULL;
    IF COL_LENGTH('cm.ChangeRequest', 'ApprovalRequired') IS NULL ALTER TABLE [cm].[ChangeRequest] ADD [ApprovalRequired] BIT NOT NULL CONSTRAINT [DF_ChangeRequest_ApprovalRequired] DEFAULT(0);
    IF COL_LENGTH('cm.ChangeRequest', 'ApprovalStrategy') IS NULL ALTER TABLE [cm].[ChangeRequest] ADD [ApprovalStrategy] NVARCHAR(50) NULL;
    IF COL_LENGTH('cm.ChangeRequest', 'ImplementationGroup') IS NULL ALTER TABLE [cm].[ChangeRequest] ADD [ImplementationGroup] NVARCHAR(200) NULL;
    IF COL_LENGTH('cm.ChangeRequest', 'SubmittedAt') IS NULL ALTER TABLE [cm].[ChangeRequest] ADD [SubmittedAt] DATETIME2 NULL;
    IF COL_LENGTH('cm.ChangeRequest', 'SubmittedByUserId') IS NULL ALTER TABLE [cm].[ChangeRequest] ADD [SubmittedByUserId] UNIQUEIDENTIFIER NULL;
    IF COL_LENGTH('cm.ChangeRequest', 'DeletedAt') IS NULL ALTER TABLE [cm].[ChangeRequest] ADD [DeletedAt] DATETIME2 NULL;
    IF COL_LENGTH('cm.ChangeRequest', 'DeletedByUserId') IS NULL ALTER TABLE [cm].[ChangeRequest] ADD [DeletedByUserId] UNIQUEIDENTIFIER NULL;
    IF COL_LENGTH('cm.ChangeRequest', 'DeletedReason') IS NULL ALTER TABLE [cm].[ChangeRequest] ADD [DeletedReason] NVARCHAR(400) NULL;
END

IF OBJECT_ID('[cm].[ChangeApprover]', 'U') IS NOT NULL
BEGIN
    IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = 'FK_ChangeApprover_ChangeRequest_ChangeRequestId')
    ALTER TABLE [cm].[ChangeApprover] WITH CHECK ADD CONSTRAINT [FK_ChangeApprover_ChangeRequest_ChangeRequestId] FOREIGN KEY ([ChangeRequestId]) REFERENCES [cm].[ChangeRequest]([ChangeRequestId]) ON DELETE CASCADE;

    IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = 'FK_ChangeApprover_User_ApproverUserId')
    ALTER TABLE [cm].[ChangeApprover] WITH CHECK ADD CONSTRAINT [FK_ChangeApprover_User_ApproverUserId] FOREIGN KEY ([ApproverUserId]) REFERENCES [cm].[User]([UserId]);

    IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_ChangeApprover_ChangeRequestId_ApproverUserId' AND object_id = OBJECT_ID('[cm].[ChangeApprover]'))
    CREATE UNIQUE INDEX [IX_ChangeApprover_ChangeRequestId_ApproverUserId] ON [cm].[ChangeApprover]([ChangeRequestId], [ApproverUserId]);
END

IF OBJECT_ID('[cm].[ChangeApproval]', 'U') IS NOT NULL
BEGIN
    IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = 'FK_ChangeApproval_ChangeRequest_ChangeRequestId')
    ALTER TABLE [cm].[ChangeApproval] WITH CHECK ADD CONSTRAINT [FK_ChangeApproval_ChangeRequest_ChangeRequestId] FOREIGN KEY ([ChangeRequestId]) REFERENCES [cm].[ChangeRequest]([ChangeRequestId]) ON DELETE CASCADE;

    IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = 'FK_ChangeApproval_User_ApproverUserId')
    ALTER TABLE [cm].[ChangeApproval] WITH CHECK ADD CONSTRAINT [FK_ChangeApproval_User_ApproverUserId] FOREIGN KEY ([ApproverUserId]) REFERENCES [cm].[User]([UserId]);

    IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = 'FK_ChangeApproval_ApprovalStatus_ApprovalStatusId')
    ALTER TABLE [cm].[ChangeApproval] WITH CHECK ADD CONSTRAINT [FK_ChangeApproval_ApprovalStatus_ApprovalStatusId] FOREIGN KEY ([ApprovalStatusId]) REFERENCES [ref].[ApprovalStatus]([ApprovalStatusId]);
END

IF OBJECT_ID('[cm].[ChangeAttachment]', 'U') IS NOT NULL
BEGIN
    IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = 'FK_ChangeAttachment_User_UploadedBy')
    ALTER TABLE [cm].[ChangeAttachment] WITH CHECK ADD CONSTRAINT [FK_ChangeAttachment_User_UploadedBy] FOREIGN KEY ([UploadedBy]) REFERENCES [cm].[User]([UserId]);
END
");
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        // Intentionally left empty: this Down() is a reconciliation migration rollback boundary.
        // The Up() path is additive/idempotent across drifted environments, and selective teardown is unsafe.
    }
}
