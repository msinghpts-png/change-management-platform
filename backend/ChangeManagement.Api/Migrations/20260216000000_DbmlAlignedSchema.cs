using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ChangeManagement.Api.Migrations
{
    public partial class DbmlAlignedSchema : Migration
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
                    table.ForeignKey("FK_Event_EventType_EventTypeId", x => x.EventTypeId, "audit", "EventType", principalColumn: "EventTypeId", onDelete: ReferentialAction.Cascade);
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
                    table.ForeignKey("FK_ChangeRequest_User_AssignedToUserId", x => x.AssignedToUserId, "cm", "User", principalColumn: "UserId", onDelete: ReferentialAction.Restrict);
                    table.ForeignKey("FK_ChangeRequest_User_RequestedByUserId", x => x.RequestedByUserId, "cm", "User", principalColumn: "UserId", onDelete: ReferentialAction.Restrict);
                    table.ForeignKey("FK_ChangeRequest_ChangeType_ChangeTypeId", x => x.ChangeTypeId, "ref", "ChangeType", principalColumn: "ChangeTypeId", onDelete: ReferentialAction.Cascade);
                    table.ForeignKey("FK_ChangeRequest_ChangePriority_PriorityId", x => x.PriorityId, "ref", "ChangePriority", principalColumn: "PriorityId", onDelete: ReferentialAction.Cascade);
                    table.ForeignKey("FK_ChangeRequest_ChangeStatus_StatusId", x => x.StatusId, "ref", "ChangeStatus", principalColumn: "StatusId", onDelete: ReferentialAction.Cascade);
                    table.ForeignKey("FK_ChangeRequest_RiskLevel_RiskLevelId", x => x.RiskLevelId, "ref", "RiskLevel", principalColumn: "RiskLevelId", onDelete: ReferentialAction.Cascade);
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
                    table.ForeignKey("FK_ChangeApproval_ApprovalStatus_ApprovalStatusId", x => x.ApprovalStatusId, "ref", "ApprovalStatus", principalColumn: "ApprovalStatusId", onDelete: ReferentialAction.Cascade);
                    table.ForeignKey("FK_ChangeApproval_ChangeRequest_ChangeRequestId", x => x.ChangeRequestId, "cm", "ChangeRequest", principalColumn: "ChangeRequestId", onDelete: ReferentialAction.Cascade);
                    table.ForeignKey("FK_ChangeApproval_User_ApproverUserId", x => x.ApproverUserId, "cm", "User", principalColumn: "UserId", onDelete: ReferentialAction.Restrict);
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
                    table.ForeignKey("FK_ChangeAttachment_ChangeRequest_ChangeRequestId", x => x.ChangeRequestId, "cm", "ChangeRequest", principalColumn: "ChangeRequestId", onDelete: ReferentialAction.Cascade);
                    table.ForeignKey("FK_ChangeAttachment_User_UploadedBy", x => x.UploadedBy, "cm", "User", principalColumn: "UserId", onDelete: ReferentialAction.Restrict);
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
                    table.ForeignKey("FK_ChangeTask_ChangeRequest_ChangeRequestId", x => x.ChangeRequestId, "cm", "ChangeRequest", principalColumn: "ChangeRequestId", onDelete: ReferentialAction.Cascade);
                    table.ForeignKey("FK_ChangeTask_ChangeStatus_StatusId", x => x.StatusId, "ref", "ChangeStatus", principalColumn: "StatusId", onDelete: ReferentialAction.Cascade);
                    table.ForeignKey("FK_ChangeTask_User_AssignedToUserId", x => x.AssignedToUserId, "cm", "User", principalColumn: "UserId", onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(name: "IX_ChangeRequest_ChangeNumber", schema: "cm", table: "ChangeRequest", column: "ChangeNumber", unique: true);

            migrationBuilder.InsertData(schema: "ref", table: "ChangeType", columns: new[] { "ChangeTypeId", "Name", "Description" }, values: new object[,]
            {
                { 1, "Standard", "Pre-approved repeatable change" },
                { 2, "Normal", "Normal CAB-reviewed change" },
                { 3, "Emergency", "Emergency expedited change" }
            });

            migrationBuilder.InsertData(schema: "ref", table: "ChangePriority", columns: new[] { "PriorityId", "Name", "SortOrder" }, values: new object[,]
            {
                { 1, "Low", 1 },
                { 2, "Medium", 2 },
                { 3, "High", 3 },
                { 4, "Critical", 4 }
            });

            migrationBuilder.InsertData(schema: "ref", table: "ChangeStatus", columns: new[] { "StatusId", "Name", "IsTerminal" }, values: new object[,]
            {
                { 1, "Draft", false },
                { 2, "Submitted", false },
                { 3, "Approved", false },
                { 4, "Rejected", true },
                { 5, "Completed", true }
            });

            migrationBuilder.InsertData(schema: "ref", table: "RiskLevel", columns: new[] { "RiskLevelId", "Name", "Score" }, values: new object[,]
            {
                { 1, "Low", 1 },
                { 2, "Medium", 5 },
                { 3, "High", 8 }
            });

            migrationBuilder.InsertData(schema: "ref", table: "ApprovalStatus", columns: new[] { "ApprovalStatusId", "Name" }, values: new object[,]
            {
                { 1, "Pending" },
                { 2, "Approved" },
                { 3, "Rejected" }
            });

            migrationBuilder.InsertData(schema: "audit", table: "EventType", columns: new[] { "EventTypeId", "Name", "Description" }, values: new object[,]
            {
                { 1, "ChangeCreated", "Change request created" },
                { 2, "ChangeUpdated", "Change request updated" },
                { 3, "ChangeSubmitted", "Change submitted for approval" },
                { 4, "ApprovalDecision", "Approval decision recorded" },
                { 5, "AttachmentUploaded", "Attachment uploaded" },
                { 6, "TemplateCreated", "Template created" },
                { 7, "TemplateUpdated", "Template updated" }
            });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
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
