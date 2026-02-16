using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ChangeManagement.Api.Migrations;

[Migration("20260217000000_DevMode_Structural_Consolidation")]
public partial class DevMode_Structural_Consolidation : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "ChangeTemplate",
            schema: "cm",
            columns: table => new
            {
                TemplateId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                Description = table.Column<string>(type: "nvarchar(max)", nullable: true),
                ImplementationSteps = table.Column<string>(type: "nvarchar(max)", nullable: true),
                BackoutPlan = table.Column<string>(type: "nvarchar(max)", nullable: true),
                ServiceSystem = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                Category = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                Environment = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                BusinessJustification = table.Column<string>(type: "nvarchar(max)", nullable: true),
                CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()"),
                CreatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_ChangeTemplate", x => x.TemplateId);
                table.ForeignKey(
                    name: "FK_ChangeTemplate_User_CreatedBy",
                    column: x => x.CreatedBy,
                    principalSchema: "cm",
                    principalTable: "User",
                    principalColumn: "UserId",
                    onDelete: ReferentialAction.Restrict);
            });

        migrationBuilder.AlterColumn<string>(
            name: "FileName",
            schema: "cm",
            table: "ChangeAttachment",
            type: "nvarchar(255)",
            maxLength: 255,
            nullable: false,
            oldClrType: typeof(string),
            oldType: "nvarchar(max)");

        migrationBuilder.AlterColumn<string>(
            name: "FileUrl",
            schema: "cm",
            table: "ChangeAttachment",
            type: "nvarchar(500)",
            maxLength: 500,
            nullable: false,
            oldClrType: typeof(string),
            oldType: "nvarchar(max)");

        migrationBuilder.AlterColumn<Guid>(
            name: "UploadedBy",
            schema: "cm",
            table: "ChangeAttachment",
            type: "uniqueidentifier",
            nullable: true,
            oldClrType: typeof(Guid),
            oldType: "uniqueidentifier");

        migrationBuilder.AddColumn<long>(
            name: "FileSizeBytes",
            schema: "cm",
            table: "ChangeAttachment",
            type: "bigint",
            nullable: false,
            defaultValue: 0L);

        migrationBuilder.CreateIndex(
            name: "IX_ChangeTemplate_CreatedBy",
            schema: "cm",
            table: "ChangeTemplate",
            column: "CreatedBy");

        migrationBuilder.CreateIndex(
            name: "IX_ChangeTemplate_IsActive",
            schema: "cm",
            table: "ChangeTemplate",
            column: "IsActive");

        migrationBuilder.CreateIndex(
            name: "IX_ChangeTemplate_Name",
            schema: "cm",
            table: "ChangeTemplate",
            column: "Name");

        if (ActiveProvider.Contains("SqlServer", StringComparison.OrdinalIgnoreCase))
        {
            migrationBuilder.Sql("IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_ChangeRequest_StatusId' AND object_id = OBJECT_ID('[cm].[ChangeRequest]')) CREATE INDEX [IX_ChangeRequest_StatusId] ON [cm].[ChangeRequest]([StatusId]);");
            migrationBuilder.Sql("IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_ChangeRequest_AssignedToUserId' AND object_id = OBJECT_ID('[cm].[ChangeRequest]')) CREATE INDEX [IX_ChangeRequest_AssignedToUserId] ON [cm].[ChangeRequest]([AssignedToUserId]);");
            migrationBuilder.Sql("IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_Event_EntityId' AND object_id = OBJECT_ID('[audit].[Event]')) CREATE INDEX [IX_Event_EntityId] ON [audit].[Event]([EntityId]);");
            migrationBuilder.Sql("IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_Event_EventTypeId' AND object_id = OBJECT_ID('[audit].[Event]')) CREATE INDEX [IX_Event_EventTypeId] ON [audit].[Event]([EventTypeId]);");
        }
        else
        {
            migrationBuilder.Sql("CREATE INDEX IF NOT EXISTS IX_ChangeRequest_StatusId ON ChangeRequest (StatusId);");
            migrationBuilder.Sql("CREATE INDEX IF NOT EXISTS IX_ChangeRequest_AssignedToUserId ON ChangeRequest (AssignedToUserId);");
            migrationBuilder.Sql("CREATE INDEX IF NOT EXISTS IX_Event_EntityId ON Event (EntityId);");
            migrationBuilder.Sql("CREATE INDEX IF NOT EXISTS IX_Event_EventTypeId ON Event (EventTypeId);");
        }
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(
            name: "ChangeTemplate",
            schema: "cm");

        migrationBuilder.DropColumn(
            name: "FileSizeBytes",
            schema: "cm",
            table: "ChangeAttachment");

        migrationBuilder.AlterColumn<Guid>(
            name: "UploadedBy",
            schema: "cm",
            table: "ChangeAttachment",
            type: "uniqueidentifier",
            nullable: false,
            defaultValue: Guid.Empty,
            oldClrType: typeof(Guid),
            oldType: "uniqueidentifier",
            oldNullable: true);

        migrationBuilder.AlterColumn<string>(
            name: "FileName",
            schema: "cm",
            table: "ChangeAttachment",
            type: "nvarchar(max)",
            nullable: false,
            oldClrType: typeof(string),
            oldType: "nvarchar(255)",
            oldMaxLength: 255);

        migrationBuilder.AlterColumn<string>(
            name: "FileUrl",
            schema: "cm",
            table: "ChangeAttachment",
            type: "nvarchar(max)",
            nullable: false,
            oldClrType: typeof(string),
            oldType: "nvarchar(500)",
            oldMaxLength: 500);
    }
}
