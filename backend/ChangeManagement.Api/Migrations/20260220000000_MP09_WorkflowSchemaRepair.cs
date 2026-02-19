using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ChangeManagement.Api.Migrations;

[Migration("20260220000000_MP09_WorkflowSchemaRepair")]
public partial class MP09WorkflowSchemaRepair : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.EnsureSchema(name: "cm");

        migrationBuilder.Sql(@"
IF OBJECT_ID('[cm].[ChangeApprover]', 'U') IS NULL
BEGIN
    CREATE TABLE [cm].[ChangeApprover]
    (
        [ChangeApproverId] UNIQUEIDENTIFIER NOT NULL,
        [ChangeRequestId] UNIQUEIDENTIFIER NOT NULL,
        [ApproverUserId] UNIQUEIDENTIFIER NOT NULL,
        [CreatedAt] DATETIME2 NOT NULL CONSTRAINT [DF_ChangeApprover_CreatedAt] DEFAULT (SYSUTCDATETIME()),
        CONSTRAINT [PK_ChangeApprover] PRIMARY KEY ([ChangeApproverId])
    );
END

IF COL_LENGTH('cm.ChangeRequest', 'ApprovalRequired') IS NULL
BEGIN
    ALTER TABLE [cm].[ChangeRequest] ADD [ApprovalRequired] BIT NOT NULL CONSTRAINT [DF_ChangeRequest_ApprovalRequired] DEFAULT(0);
END

IF COL_LENGTH('cm.ChangeRequest', 'ApprovalRequesterUserId') IS NULL
BEGIN
    ALTER TABLE [cm].[ChangeRequest] ADD [ApprovalRequesterUserId] UNIQUEIDENTIFIER NULL;
END

IF COL_LENGTH('cm.ChangeRequest', 'ApprovalStrategy') IS NULL
BEGIN
    ALTER TABLE [cm].[ChangeRequest] ADD [ApprovalStrategy] NVARCHAR(50) NULL;
END
ELSE
BEGIN
    ALTER TABLE [cm].[ChangeRequest] ALTER COLUMN [ApprovalStrategy] NVARCHAR(50) NULL;
END

IF OBJECT_ID('[cm].[ChangeApproval]', 'U') IS NOT NULL
AND NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = 'FK_ChangeApproval_ChangeRequest_ChangeRequestId')
BEGIN
    ALTER TABLE [cm].[ChangeApproval] WITH CHECK ADD CONSTRAINT [FK_ChangeApproval_ChangeRequest_ChangeRequestId]
        FOREIGN KEY ([ChangeRequestId]) REFERENCES [cm].[ChangeRequest]([ChangeRequestId]) ON DELETE CASCADE;
END

IF OBJECT_ID('[cm].[ChangeApproval]', 'U') IS NOT NULL
AND NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = 'FK_ChangeApproval_ApprovalStatus_ApprovalStatusId')
BEGIN
    ALTER TABLE [cm].[ChangeApproval] WITH CHECK ADD CONSTRAINT [FK_ChangeApproval_ApprovalStatus_ApprovalStatusId]
        FOREIGN KEY ([ApprovalStatusId]) REFERENCES [ref].[ApprovalStatus]([ApprovalStatusId]) ON DELETE CASCADE;
END

IF OBJECT_ID('[cm].[ChangeApproval]', 'U') IS NOT NULL
AND NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = 'FK_ChangeApproval_User_ApproverUserId')
BEGIN
    ALTER TABLE [cm].[ChangeApproval] WITH CHECK ADD CONSTRAINT [FK_ChangeApproval_User_ApproverUserId]
        FOREIGN KEY ([ApproverUserId]) REFERENCES [cm].[User]([UserId]) ON DELETE NO ACTION;
END

IF OBJECT_ID('[cm].[ChangeApprover]', 'U') IS NOT NULL
AND NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = 'FK_ChangeApprover_ChangeRequest_ChangeRequestId')
BEGIN
    ALTER TABLE [cm].[ChangeApprover] WITH CHECK ADD CONSTRAINT [FK_ChangeApprover_ChangeRequest_ChangeRequestId]
        FOREIGN KEY ([ChangeRequestId]) REFERENCES [cm].[ChangeRequest]([ChangeRequestId]) ON DELETE CASCADE;
END

IF OBJECT_ID('[cm].[ChangeApprover]', 'U') IS NOT NULL
AND NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = 'FK_ChangeApprover_User_ApproverUserId')
BEGIN
    ALTER TABLE [cm].[ChangeApprover] WITH CHECK ADD CONSTRAINT [FK_ChangeApprover_User_ApproverUserId]
        FOREIGN KEY ([ApproverUserId]) REFERENCES [cm].[User]([UserId]) ON DELETE NO ACTION;
END

IF OBJECT_ID('[cm].[ChangeApproval]', 'U') IS NOT NULL
AND NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_ChangeApproval_ChangeRequestId' AND object_id = OBJECT_ID('[cm].[ChangeApproval]'))
BEGIN
    CREATE INDEX [IX_ChangeApproval_ChangeRequestId] ON [cm].[ChangeApproval]([ChangeRequestId]);
END

IF OBJECT_ID('[cm].[ChangeApproval]', 'U') IS NOT NULL
AND NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_ChangeApproval_ApproverUserId' AND object_id = OBJECT_ID('[cm].[ChangeApproval]'))
BEGIN
    CREATE INDEX [IX_ChangeApproval_ApproverUserId] ON [cm].[ChangeApproval]([ApproverUserId]);
END

IF OBJECT_ID('[cm].[ChangeApprover]', 'U') IS NOT NULL
AND NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_ChangeApprover_ChangeRequestId' AND object_id = OBJECT_ID('[cm].[ChangeApprover]'))
BEGIN
    CREATE INDEX [IX_ChangeApprover_ChangeRequestId] ON [cm].[ChangeApprover]([ChangeRequestId]);
END

IF OBJECT_ID('[cm].[ChangeApprover]', 'U') IS NOT NULL
AND NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_ChangeApprover_ApproverUserId' AND object_id = OBJECT_ID('[cm].[ChangeApprover]'))
BEGIN
    CREATE INDEX [IX_ChangeApprover_ApproverUserId] ON [cm].[ChangeApprover]([ApproverUserId]);
END

IF OBJECT_ID('[cm].[ChangeApprover]', 'U') IS NOT NULL
AND NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_ChangeApprover_ChangeRequestId_ApproverUserId' AND object_id = OBJECT_ID('[cm].[ChangeApprover]'))
BEGIN
    CREATE UNIQUE INDEX [IX_ChangeApprover_ChangeRequestId_ApproverUserId] ON [cm].[ChangeApprover]([ChangeRequestId], [ApproverUserId]);
END
");
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql(@"
IF OBJECT_ID('[cm].[ChangeApprover]', 'U') IS NOT NULL
BEGIN
    DROP TABLE [cm].[ChangeApprover];
END
");
    }
}
