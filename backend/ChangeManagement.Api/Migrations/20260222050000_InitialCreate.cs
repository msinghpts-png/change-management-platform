using System;
using ChangeManagement.Api.Data;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ChangeManagement.Api.Migrations
{
    [DbContext(typeof(ChangeManagementDbContext))]
    [Migration("20260222050000_InitialCreate")]
    public partial class InitialCreate : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(name: "cm");
            migrationBuilder.EnsureSchema(name: "ref");
            migrationBuilder.EnsureSchema(name: "audit");

            migrationBuilder.CreateSequence<int>(name: "ChangeNumberSeq", schema: "cm", startValue: 1000L);

            migrationBuilder.CreateTable(
                name: "User",
                schema: "cm",
                columns: table => new
                {
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Upn = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    DisplayName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Role = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table => table.PrimaryKey("PK_User", x => x.UserId));

            migrationBuilder.CreateTable(
                name: "EventType",
                schema: "audit",
                columns: table => new
                {
                    EventTypeId = table.Column<int>(type: "int", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table => table.PrimaryKey("PK_EventType", x => x.EventTypeId));

            migrationBuilder.CreateTable(
                name: "ApprovalStatus",
                schema: "ref",
                columns: table => new
                {
                    ApprovalStatusId = table.Column<int>(type: "int", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table => table.PrimaryKey("PK_ApprovalStatus", x => x.ApprovalStatusId));

            migrationBuilder.CreateTable(
                name: "ChangePriority",
                schema: "ref",
                columns: table => new
                {
                    PriorityId = table.Column<int>(type: "int", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    SortOrder = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table => table.PrimaryKey("PK_ChangePriority", x => x.PriorityId));

            migrationBuilder.CreateTable(
                name: "ChangeStatus",
                schema: "ref",
                columns: table => new
                {
                    StatusId = table.Column<int>(type: "int", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IsTerminal = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table => table.PrimaryKey("PK_ChangeStatus", x => x.StatusId));

            migrationBuilder.CreateTable(
                name: "ChangeType",
                schema: "ref",
                columns: table => new
                {
                    ChangeTypeId = table.Column<int>(type: "int", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table => table.PrimaryKey("PK_ChangeType", x => x.ChangeTypeId));

            migrationBuilder.CreateTable(
                name: "RiskLevel",
                schema: "ref",
                columns: table => new
                {
                    RiskLevelId = table.Column<int>(type: "int", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Score = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table => table.PrimaryKey("PK_RiskLevel", x => x.RiskLevelId));

            migrationBuilder.CreateTable(
                name: "Event",
                schema: "audit",
                columns: table => new
                {
                    AuditEventId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    EventTypeId = table.Column<int>(type: "int", nullable: false),
                    EventAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ActorUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ActorUpn = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    EntitySchema = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    EntityName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    EntityId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ChangeNumber = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Reason = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Details = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Event", x => x.AuditEventId);
                    table.ForeignKey("FK_Event_EventType_EventTypeId", x => x.EventTypeId, "EventType", "EventTypeId", "audit", onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ChangeRequest",
                schema: "cm",
                columns: table => new
                {
                    ChangeRequestId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ChangeNumber = table.Column<int>(type: "int", nullable: false, defaultValueSql: "NEXT VALUE FOR cm.ChangeNumberSeq"),
                    Title = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ChangeTypeId = table.Column<int>(type: "int", nullable: false),
                    PriorityId = table.Column<int>(type: "int", nullable: false),
                    StatusId = table.Column<int>(type: "int", nullable: false),
                    RiskLevelId = table.Column<int>(type: "int", nullable: false),
                    RequestedByUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    AssignedToUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    PlannedStart = table.Column<DateTime>(type: "datetime2", nullable: true),
                    PlannedEnd = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ActualStart = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ActualEnd = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ChangeRequest", x => x.ChangeRequestId);
                    table.ForeignKey("FK_ChangeRequest_User_AssignedToUserId", x => x.AssignedToUserId, "User", "UserId", "cm", onDelete: ReferentialAction.Restrict);
                    table.ForeignKey("FK_ChangeRequest_User_RequestedByUserId", x => x.RequestedByUserId, "User", "UserId", "cm", onDelete: ReferentialAction.Restrict);
                    table.ForeignKey("FK_ChangeRequest_ChangeType_ChangeTypeId", x => x.ChangeTypeId, "ChangeType", "ChangeTypeId", "ref", onDelete: ReferentialAction.Cascade);
                    table.ForeignKey("FK_ChangeRequest_ChangePriority_PriorityId", x => x.PriorityId, "ChangePriority", "PriorityId", "ref", onDelete: ReferentialAction.Cascade);
                    table.ForeignKey("FK_ChangeRequest_ChangeStatus_StatusId", x => x.StatusId, "ChangeStatus", "StatusId", "ref", onDelete: ReferentialAction.Cascade);
                    table.ForeignKey("FK_ChangeRequest_RiskLevel_RiskLevelId", x => x.RiskLevelId, "RiskLevel", "RiskLevelId", "ref", onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ChangeApproval",
                schema: "cm",
                columns: table => new
                {
                    ChangeApprovalId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ChangeRequestId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ApproverUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ApprovalStatusId = table.Column<int>(type: "int", nullable: false),
                    ApprovedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Comments = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ChangeApproval", x => x.ChangeApprovalId);
                    table.ForeignKey("FK_ChangeApproval_ApprovalStatus_ApprovalStatusId", x => x.ApprovalStatusId, "ApprovalStatus", "ApprovalStatusId", "ref", onDelete: ReferentialAction.Cascade);
                    table.ForeignKey("FK_ChangeApproval_ChangeRequest_ChangeRequestId", x => x.ChangeRequestId, "ChangeRequest", "ChangeRequestId", "cm", onDelete: ReferentialAction.Cascade);
                    table.ForeignKey("FK_ChangeApproval_User_ApproverUserId", x => x.ApproverUserId, "User", "UserId", "cm", onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ChangeAttachment",
                schema: "cm",
                columns: table => new
                {
                    ChangeAttachmentId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ChangeRequestId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    FileName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    FileUrl = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    UploadedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UploadedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ChangeAttachment", x => x.ChangeAttachmentId);
                    table.ForeignKey("FK_ChangeAttachment_ChangeRequest_ChangeRequestId", x => x.ChangeRequestId, "ChangeRequest", "ChangeRequestId", "cm", onDelete: ReferentialAction.Cascade);
                    table.ForeignKey("FK_ChangeAttachment_User_UploadedBy", x => x.UploadedBy, "User", "UserId", "cm", onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ChangeTask",
                schema: "cm",
                columns: table => new
                {
                    ChangeTaskId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ChangeRequestId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Title = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    StatusId = table.Column<int>(type: "int", nullable: false),
                    AssignedToUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    DueAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CompletedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ChangeTask", x => x.ChangeTaskId);
                    table.ForeignKey("FK_ChangeTask_ChangeRequest_ChangeRequestId", x => x.ChangeRequestId, "ChangeRequest", "ChangeRequestId", "cm", onDelete: ReferentialAction.Cascade);
                    table.ForeignKey("FK_ChangeTask_ChangeStatus_StatusId", x => x.StatusId, "ChangeStatus", "StatusId", "ref", onDelete: ReferentialAction.Restrict);
                    table.ForeignKey("FK_ChangeTask_User_AssignedToUserId", x => x.AssignedToUserId, "User", "UserId", "cm", onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(name: "IX_ChangeRequest_ChangeNumber", schema: "cm", table: "ChangeRequest", column: "ChangeNumber", unique: true);

            migrationBuilder.Sql(@"
IF NOT EXISTS (SELECT 1 FROM [ref].[ChangeType])
BEGIN
    INSERT INTO [ref].[ChangeType] ([ChangeTypeId], [Name], [Description]) VALUES
    (1, 'Standard', 'Pre-approved repeatable change'),
    (2, 'Normal', 'Normal CAB-reviewed change'),
    (3, 'Emergency', 'Emergency expedited change');
END;");

            migrationBuilder.Sql(@"
IF NOT EXISTS (SELECT 1 FROM [ref].[ChangePriority])
BEGIN
    INSERT INTO [ref].[ChangePriority] ([PriorityId], [Name], [SortOrder]) VALUES
    (1, 'Low', 1),
    (2, 'Medium', 2),
    (3, 'High', 3),
    (4, 'Critical', 4);
END;");

            migrationBuilder.Sql(@"
IF NOT EXISTS (SELECT 1 FROM [ref].[ChangeStatus])
BEGIN
    INSERT INTO [ref].[ChangeStatus] ([StatusId], [Name], [IsTerminal]) VALUES
    (1, 'Draft', 0),
    (2, 'Submitted', 0),
    (3, 'Approved', 0),
    (4, 'Rejected', 1),
    (5, 'Completed', 1);
END;");

            migrationBuilder.Sql(@"
IF NOT EXISTS (SELECT 1 FROM [ref].[RiskLevel])
BEGIN
    INSERT INTO [ref].[RiskLevel] ([RiskLevelId], [Name], [Score]) VALUES
    (1, 'Low', 1),
    (2, 'Medium', 5),
    (3, 'High', 8);
END;");

            migrationBuilder.Sql(@"
IF NOT EXISTS (SELECT 1 FROM [ref].[ApprovalStatus])
BEGIN
    INSERT INTO [ref].[ApprovalStatus] ([ApprovalStatusId], [Name]) VALUES
    (1, 'Pending'),
    (2, 'Approved'),
    (3, 'Rejected');
END;");

            migrationBuilder.Sql(@"
IF NOT EXISTS (SELECT 1 FROM [audit].[EventType])
BEGIN
    INSERT INTO [audit].[EventType] ([EventTypeId], [Name], [Description]) VALUES
    (1, 'ChangeCreated', 'Change request created'),
    (2, 'ChangeUpdated', 'Change request updated'),
    (3, 'ChangeSubmitted', 'Change submitted for approval'),
    (4, 'ApprovalDecision', 'Approval decision recorded'),
    (5, 'AttachmentUploaded', 'Attachment uploaded'),
    (6, 'TemplateCreated', 'Template created'),
    (7, 'TemplateUpdated', 'Template updated');
END;");


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

IF COL_LENGTH('cm.User', 'PasswordHash') IS NULL
BEGIN
    ALTER TABLE [cm].[User] ADD [PasswordHash] NVARCHAR(MAX) NOT NULL CONSTRAINT [DF_User_PasswordHash] DEFAULT('');
END

IF COL_LENGTH('cm.ChangeAttachment', 'FilePath') IS NULL
BEGIN
    ALTER TABLE [cm].[ChangeAttachment] ADD [FilePath] NVARCHAR(500) NOT NULL CONSTRAINT [DF_ChangeAttachment_FilePath] DEFAULT('');
END
IF COL_LENGTH('cm.ChangeAttachment', 'FileSizeBytes') IS NULL
BEGIN
    ALTER TABLE [cm].[ChangeAttachment] ADD [FileSizeBytes] BIGINT NOT NULL CONSTRAINT [DF_ChangeAttachment_FileSizeBytes] DEFAULT(0);
END
IF COL_LENGTH('cm.ChangeAttachment', 'UploadedBy') IS NOT NULL
BEGIN
    BEGIN TRY
        ALTER TABLE [cm].[ChangeAttachment] ALTER COLUMN [UploadedBy] UNIQUEIDENTIFIER NULL;
    END TRY
    BEGIN CATCH
    END CATCH
END

IF OBJECT_ID('[cm].[ChangeTemplate]', 'U') IS NULL
BEGIN
    CREATE TABLE [cm].[ChangeTemplate](
        [TemplateId] UNIQUEIDENTIFIER NOT NULL PRIMARY KEY,
        [Name] NVARCHAR(200) NOT NULL,
        [Description] NVARCHAR(MAX) NULL,
        [ImplementationSteps] NVARCHAR(MAX) NULL,
        [BackoutPlan] NVARCHAR(MAX) NULL,
        [ServiceSystem] NVARCHAR(200) NULL,
        [Category] NVARCHAR(200) NULL,
        [Environment] NVARCHAR(200) NULL,
        [BusinessJustification] NVARCHAR(MAX) NULL,
        [ChangeTypeId] INT NULL,
        [RiskLevelId] INT NULL,
        [CreatedAt] DATETIME2 NOT NULL DEFAULT(GETUTCDATE()),
        [CreatedBy] UNIQUEIDENTIFIER NOT NULL,
        [IsActive] BIT NOT NULL DEFAULT(1)
    );
END
");

        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(name: "ChangeApprover", schema: "cm");
            migrationBuilder.DropTable(name: "ChangeTemplate", schema: "cm");
            migrationBuilder.DropTable(name: "ChangeAttachment", schema: "cm");
            migrationBuilder.DropTable(name: "ChangeApproval", schema: "cm");
            migrationBuilder.DropTable(name: "ChangeTask", schema: "cm");
            migrationBuilder.DropTable(name: "Event", schema: "audit");
            migrationBuilder.DropTable(name: "ChangeRequest", schema: "cm");
            migrationBuilder.DropTable(name: "ApprovalStatus", schema: "ref");
            migrationBuilder.DropTable(name: "EventType", schema: "audit");
            migrationBuilder.DropTable(name: "ChangeType", schema: "ref");
            migrationBuilder.DropTable(name: "ChangePriority", schema: "ref");
            migrationBuilder.DropTable(name: "ChangeStatus", schema: "ref");
            migrationBuilder.DropTable(name: "RiskLevel", schema: "ref");
            migrationBuilder.DropTable(name: "User", schema: "cm");
            migrationBuilder.DropSequence(name: "ChangeNumberSeq", schema: "cm");
        }
    }
}
