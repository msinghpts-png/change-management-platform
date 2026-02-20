using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ChangeManagement.Api.Migrations;

[Migration("20260222000000_AddChangeApproverTable")]
public partial class AddChangeApproverTable : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
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
        CONSTRAINT [FK_ChangeApprover_ChangeRequest_ChangeRequestId] 
            FOREIGN KEY ([ChangeRequestId]) REFERENCES [cm].[ChangeRequest]([ChangeRequestId]) ON DELETE CASCADE,
        CONSTRAINT [FK_ChangeApprover_User_ApproverUserId] 
            FOREIGN KEY ([ApproverUserId]) REFERENCES [cm].[User]([UserId])
    );

    CREATE INDEX [IX_ChangeApprover_ChangeRequestId] ON [cm].[ChangeApprover]([ChangeRequestId]);
    CREATE INDEX [IX_ChangeApprover_ApproverUserId] ON [cm].[ChangeApprover]([ApproverUserId]);
    CREATE UNIQUE INDEX [IX_ChangeApprover_ChangeRequestId_ApproverUserId] 
        ON [cm].[ChangeApprover]([ChangeRequestId], [ApproverUserId]);
END
");
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql("DROP TABLE IF EXISTS [cm].[ChangeApprover];");
    }
}
