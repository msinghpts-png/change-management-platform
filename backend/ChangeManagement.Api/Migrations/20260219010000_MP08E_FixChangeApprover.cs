using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ChangeManagement.Api.Migrations;

[Migration("20260219010000_MP08E_FixChangeApprover")]
public partial class MP08EFixChangeApprover : Migration
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
        CONSTRAINT [PK_ChangeApprover] PRIMARY KEY ([ChangeApproverId]),
        CONSTRAINT [FK_ChangeApprover_ChangeRequest_ChangeRequestId] FOREIGN KEY ([ChangeRequestId]) REFERENCES [cm].[ChangeRequest]([ChangeRequestId]) ON DELETE CASCADE,
        CONSTRAINT [FK_ChangeApprover_User_ApproverUserId] FOREIGN KEY ([ApproverUserId]) REFERENCES [cm].[User]([UserId]) ON DELETE NO ACTION
    );

    CREATE INDEX [IX_ChangeApprover_ChangeRequestId] ON [cm].[ChangeApprover]([ChangeRequestId]);
    CREATE INDEX [IX_ChangeApprover_ApproverUserId] ON [cm].[ChangeApprover]([ApproverUserId]);
    CREATE UNIQUE INDEX [IX_ChangeApprover_ChangeRequestId_ApproverUserId] ON [cm].[ChangeApprover]([ChangeRequestId], [ApproverUserId]);
END
ELSE
BEGIN
    IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_ChangeApprover_ChangeRequestId' AND object_id = OBJECT_ID('[cm].[ChangeApprover]'))
        CREATE INDEX [IX_ChangeApprover_ChangeRequestId] ON [cm].[ChangeApprover]([ChangeRequestId]);

    IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_ChangeApprover_ApproverUserId' AND object_id = OBJECT_ID('[cm].[ChangeApprover]'))
        CREATE INDEX [IX_ChangeApprover_ApproverUserId] ON [cm].[ChangeApprover]([ApproverUserId]);

    IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_ChangeApprover_ChangeRequestId_ApproverUserId' AND object_id = OBJECT_ID('[cm].[ChangeApprover]'))
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
