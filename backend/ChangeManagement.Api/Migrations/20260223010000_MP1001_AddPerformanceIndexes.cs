using ChangeManagement.Api.Data;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ChangeManagement.Api.Migrations;

[DbContext(typeof(ChangeManagementDbContext))]
[Migration("20260223010000_MP1001_AddPerformanceIndexes")]
public partial class MP1001_AddPerformanceIndexes : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql("""
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_ChangeRequest_StatusId' AND object_id = OBJECT_ID('[cm].[ChangeRequest]'))
    CREATE INDEX [IX_ChangeRequest_StatusId] ON [cm].[ChangeRequest] ([StatusId]);
""");

        migrationBuilder.Sql("""
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_ChangeRequest_RequestedByUserId' AND object_id = OBJECT_ID('[cm].[ChangeRequest]'))
    CREATE INDEX [IX_ChangeRequest_RequestedByUserId] ON [cm].[ChangeRequest] ([RequestedByUserId]);
""");

        migrationBuilder.Sql("""
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_ChangeRequest_ChangeNumber' AND object_id = OBJECT_ID('[cm].[ChangeRequest]'))
    CREATE UNIQUE INDEX [IX_ChangeRequest_ChangeNumber] ON [cm].[ChangeRequest] ([ChangeNumber]);
""");

        migrationBuilder.Sql("""
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_ChangeApprover_ChangeRequestId' AND object_id = OBJECT_ID('[cm].[ChangeApprover]'))
    CREATE INDEX [IX_ChangeApprover_ChangeRequestId] ON [cm].[ChangeApprover] ([ChangeRequestId]);
""");

        migrationBuilder.Sql("""
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_ChangeAttachment_ChangeRequestId' AND object_id = OBJECT_ID('[cm].[ChangeAttachment]'))
    CREATE INDEX [IX_ChangeAttachment_ChangeRequestId] ON [cm].[ChangeAttachment] ([ChangeRequestId]);
""");
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql("""
IF EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_ChangeAttachment_ChangeRequestId' AND object_id = OBJECT_ID('[cm].[ChangeAttachment]'))
    DROP INDEX [IX_ChangeAttachment_ChangeRequestId] ON [cm].[ChangeAttachment];
""");

        migrationBuilder.Sql("""
IF EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_ChangeApprover_ChangeRequestId' AND object_id = OBJECT_ID('[cm].[ChangeApprover]'))
    DROP INDEX [IX_ChangeApprover_ChangeRequestId] ON [cm].[ChangeApprover];
""");

        migrationBuilder.Sql("""
IF EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_ChangeRequest_RequestedByUserId' AND object_id = OBJECT_ID('[cm].[ChangeRequest]'))
    DROP INDEX [IX_ChangeRequest_RequestedByUserId] ON [cm].[ChangeRequest];
""");

        migrationBuilder.Sql("""
IF EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_ChangeRequest_StatusId' AND object_id = OBJECT_ID('[cm].[ChangeRequest]'))
    DROP INDEX [IX_ChangeRequest_StatusId] ON [cm].[ChangeRequest];
""");
    }
}
