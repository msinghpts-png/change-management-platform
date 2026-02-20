using System;
using ChangeManagement.Api.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ChangeManagement.Api.Migrations;

[DbContext(typeof(ChangeManagementDbContext))]
[Migration("20260223000000_MP1001_DbRebuild")]
public partial class MP1001_DbRebuild : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.EnsureSchema(name: "cm");
        migrationBuilder.EnsureSchema(name: "ref");
        migrationBuilder.EnsureSchema(name: "audit");

        // Section 1: REFERENCE (LOOKUP) TABLES + SEED DATA
        // =============================================
        // 1. REFERENCE (LOOKUP) TABLES + SEED DATA
        // =============================================
        migrationBuilder.CreateTable(
            name: "ChangeType",
            schema: "ref",
            columns: table => new
            {
                ChangeTypeId = table.Column<int>(type: "int", nullable: false),
                Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                Description = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: true)
            },
            constraints: table => table.PrimaryKey("PK_ChangeType", x => x.ChangeTypeId));

        migrationBuilder.CreateTable(
            name: "ChangePriority",
            schema: "ref",
            columns: table => new
            {
                PriorityId = table.Column<int>(type: "int", nullable: false),
                Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                SortOrder = table.Column<int>(type: "int", nullable: false)
            },
            constraints: table => table.PrimaryKey("PK_ChangePriority", x => x.PriorityId));

        migrationBuilder.CreateTable(
            name: "ChangeStatus",
            schema: "ref",
            columns: table => new
            {
                StatusId = table.Column<int>(type: "int", nullable: false),
                Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                IsTerminal = table.Column<bool>(type: "bit", nullable: false)
            },
            constraints: table => table.PrimaryKey("PK_ChangeStatus", x => x.StatusId));

        migrationBuilder.CreateTable(
            name: "RiskLevel",
            schema: "ref",
            columns: table => new
            {
                RiskLevelId = table.Column<int>(type: "int", nullable: false),
                Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                Score = table.Column<int>(type: "int", nullable: false)
            },
            constraints: table => table.PrimaryKey("PK_RiskLevel", x => x.RiskLevelId));

        migrationBuilder.CreateTable(
            name: "ImpactLevel",
            schema: "ref",
            columns: table => new
            {
                ImpactLevelId = table.Column<int>(type: "int", nullable: false),
                Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                Score = table.Column<int>(type: "int", nullable: false)
            },
            constraints: table => table.PrimaryKey("PK_ImpactLevel", x => x.ImpactLevelId));

        migrationBuilder.CreateTable(
            name: "ImpactType",
            schema: "ref",
            columns: table => new
            {
                ImpactTypeId = table.Column<int>(type: "int", nullable: false),
                Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                Description = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: true)
            },
            constraints: table => table.PrimaryKey("PK_ImpactType", x => x.ImpactTypeId));

        // Seed reference data (ITIL-inspired – adjust as needed)
        migrationBuilder.InsertData(
            schema: "ref", table: "ChangeType",
            columns: new[] { "ChangeTypeId", "Name", "Description" },
            values: new object[,]
            {
                { 1, "Standard", "Pre-approved low-risk changes" },
                { 2, "Normal",   "Standard changes requiring approval" },
                { 3, "Emergency","Urgent changes to restore service" }
            });

        migrationBuilder.InsertData(
            schema: "ref", table: "ChangePriority",
            columns: new[] { "PriorityId", "Name", "SortOrder" },
            values: new object[,]
            {
                { 1, "Critical", 1 },
                { 2, "High",     2 },
                { 3, "Medium",   3 },
                { 4, "Low",      4 }
            });

        migrationBuilder.InsertData(
            schema: "ref", table: "ChangeStatus",
            columns: new[] { "StatusId", "Name", "IsTerminal" },
            values: new object[,]
            {
                { 1, "Draft",       false },
                { 2, "Submitted",   false },
                { 3, "In Review",   false },
                { 4, "Approved",    false },
                { 5, "Scheduled",   false },
                { 6, "In Progress", false },
                { 7, "Implemented", true },
                { 8, "Closed",      true },
                { 9, "Rejected",    true },
                {10, "Failed",      true }
            });

        migrationBuilder.InsertData(
            schema: "ref", table: "RiskLevel",
            columns: new[] { "RiskLevelId", "Name", "Score" },
            values: new object[,]
            {
                { 1, "Low",      1 },
                { 2, "Medium",   3 },
                { 3, "High",     5 },
                { 4, "Critical",10 }
            });

        migrationBuilder.InsertData(
            schema: "ref", table: "ImpactLevel",
            columns: new[] { "ImpactLevelId", "Name", "Score" },
            values: new object[,]
            {
                { 1, "Very Low",  1 },
                { 2, "Low",       2 },
                { 3, "Medium",    3 },
                { 4, "High",      4 },
                { 5, "Very High", 5 }
            });

        migrationBuilder.InsertData(
            schema: "ref", table: "ImpactType",
            columns: new[] { "ImpactTypeId", "Name", "Description" },
            values: new object[,]
            {
                { 1, "Operational", "Impact on day-to-day operations" },
                { 2, "Financial",   "Financial or budget impact" },
                { 3, "Customer",    "Customer-facing service impact" },
                { 4, "Regulatory",  "Compliance or regulatory impact" },
                { 5, "Technical",   "Purely technical/system impact" }
            });

        // Section 2: SEQUENCE FOR ChangeNumber (auto-increment)
        migrationBuilder.CreateSequence<int>(
            name: "ChangeNumberSeq",
            schema: "cm",
            startValue: 10001,
            incrementBy: 1,
            minValue: 10001);

        // Section 3: CORE TABLES
        migrationBuilder.CreateTable(
            name: "User",
            schema: "cm",
            columns: table => new
            {
                UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                Upn = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                DisplayName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                PasswordHash = table.Column<string>(type: "nvarchar(max)", nullable: false),
                Role = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()")
            },
            constraints: table => table.PrimaryKey("PK_User", x => x.UserId));

        migrationBuilder.CreateTable(
            name: "ChangeRequest",
            schema: "cm",
            columns: table => new
            {
                ChangeRequestId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                ChangeNumber = table.Column<int>(type: "int", nullable: false, defaultValueSql: "NEXT VALUE FOR cm.ChangeNumberSeq"),
                Title = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: false),
                Description = table.Column<string>(type: "nvarchar(max)", nullable: true),
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
                ImplementationSteps = table.Column<string>(type: "nvarchar(max)", nullable: true),
                BackoutPlan = table.Column<string>(type: "nvarchar(max)", nullable: true),
                BusinessJustification = table.Column<string>(type: "nvarchar(max)", nullable: true),
                Category = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                Environment = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                ServiceSystem = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                ImplementationGroup = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                ImpactLevelId = table.Column<int>(type: "int", nullable: true),
                ImpactTypeId = table.Column<int>(type: "int", nullable: true),
                ApprovalRequired = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                ApprovalStrategy = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                ApprovalRequesterUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                SubmittedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                SubmittedByUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                DeletedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                DeletedByUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                DeletedReason = table.Column<string>(type: "nvarchar(400)", maxLength: 400, nullable: true),
                CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()"),
                CreatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                UpdatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_ChangeRequest", x => x.ChangeRequestId);

                table.ForeignKey("FK_ChangeRequest_ChangeType", x => x.ChangeTypeId, "ChangeType", "ChangeTypeId", "ref", onDelete: ReferentialAction.Restrict);
                table.ForeignKey("FK_ChangeRequest_ChangePriority", x => x.PriorityId, "ChangePriority", "PriorityId", "ref", onDelete: ReferentialAction.Restrict);
                table.ForeignKey("FK_ChangeRequest_ChangeStatus", x => x.StatusId, "ChangeStatus", "StatusId", "ref", onDelete: ReferentialAction.Restrict);
                table.ForeignKey("FK_ChangeRequest_RiskLevel", x => x.RiskLevelId, "RiskLevel", "RiskLevelId", "ref", onDelete: ReferentialAction.Restrict);
                table.ForeignKey("FK_ChangeRequest_ImpactLevel", x => x.ImpactLevelId, "ImpactLevel", "ImpactLevelId", "ref", onDelete: ReferentialAction.Restrict);
                table.ForeignKey("FK_ChangeRequest_ImpactType", x => x.ImpactTypeId, "ImpactType", "ImpactTypeId", "ref", onDelete: ReferentialAction.Restrict);

                table.ForeignKey("FK_ChangeRequest_RequestedBy", x => x.RequestedByUserId, "User", "UserId", "cm", onDelete: ReferentialAction.Restrict);
                table.ForeignKey("FK_ChangeRequest_AssignedTo", x => x.AssignedToUserId, "User", "UserId", "cm", onDelete: ReferentialAction.Restrict);
                table.ForeignKey("FK_ChangeRequest_SubmittedBy", x => x.SubmittedByUserId, "User", "UserId", "cm", onDelete: ReferentialAction.Restrict);
                table.ForeignKey("FK_ChangeRequest_ApprovalRequester", x => x.ApprovalRequesterUserId, "User", "UserId", "cm", onDelete: ReferentialAction.Restrict);
                table.ForeignKey("FK_ChangeRequest_DeletedBy", x => x.DeletedByUserId, "User", "UserId", "cm", onDelete: ReferentialAction.Restrict);
                table.ForeignKey("FK_ChangeRequest_CreatedBy", x => x.CreatedBy, "User", "UserId", "cm", onDelete: ReferentialAction.Restrict);
                table.ForeignKey("FK_ChangeRequest_UpdatedBy", x => x.UpdatedBy, "User", "UserId", "cm", onDelete: ReferentialAction.Restrict);
            });

        // Section 4: CHILD TABLES
        migrationBuilder.CreateTable(
            name: "ChangeApprover",
            schema: "cm",
            columns: table => new
            {
                ChangeApproverId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                ChangeRequestId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                ApproverUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                ApprovalStatus = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                DecisionAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                DecisionComments = table.Column<string>(type: "nvarchar(max)", nullable: true),
                CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()")
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_ChangeApprover", x => x.ChangeApproverId);
                table.ForeignKey("FK_ChangeApprover_ChangeRequest", x => x.ChangeRequestId, "ChangeRequest", "ChangeRequestId", "cm", onDelete: ReferentialAction.Cascade);
                table.ForeignKey("FK_ChangeApprover_User", x => x.ApproverUserId, "User", "UserId", "cm", onDelete: ReferentialAction.Restrict);
            });

        migrationBuilder.CreateTable(
            name: "ChangeAttachment",
            schema: "cm",
            columns: table => new
            {
                ChangeAttachmentId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                ChangeRequestId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                FileName = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: false),
                FilePath = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                FileSizeBytes = table.Column<long>(type: "bigint", nullable: false),
                UploadedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()"),
                UploadedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_ChangeAttachment", x => x.ChangeAttachmentId);
                table.ForeignKey("FK_ChangeAttachment_ChangeRequest", x => x.ChangeRequestId, "ChangeRequest", "ChangeRequestId", "cm", onDelete: ReferentialAction.Cascade);
                table.ForeignKey("FK_ChangeAttachment_User", x => x.UploadedBy, "User", "UserId", "cm", onDelete: ReferentialAction.Restrict);
            });

        migrationBuilder.CreateTable(
            name: "ChangeTask",
            schema: "cm",
            columns: table => new
            {
                ChangeTaskId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                ChangeRequestId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                Title = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: false),
                Description = table.Column<string>(type: "nvarchar(max)", nullable: true),
                AssignedToUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                Status = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                DueDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                CompletedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()")
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_ChangeTask", x => x.ChangeTaskId);
                table.ForeignKey("FK_ChangeTask_ChangeRequest", x => x.ChangeRequestId, "ChangeRequest", "ChangeRequestId", "cm", onDelete: ReferentialAction.Cascade);
                table.ForeignKey("FK_ChangeTask_User", x => x.AssignedToUserId, "User", "UserId", "cm", onDelete: ReferentialAction.Restrict);
            });

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
                ChangeTypeId = table.Column<int>(type: "int", nullable: true),
                RiskLevelId = table.Column<int>(type: "int", nullable: true),
                IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()"),
                CreatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_ChangeTemplate", x => x.TemplateId);
                table.ForeignKey("FK_ChangeTemplate_ChangeType", x => x.ChangeTypeId, "ChangeType", "ChangeTypeId", "ref", onDelete: ReferentialAction.Restrict);
                table.ForeignKey("FK_ChangeTemplate_RiskLevel", x => x.RiskLevelId, "RiskLevel", "RiskLevelId", "ref", onDelete: ReferentialAction.Restrict);
                table.ForeignKey("FK_ChangeTemplate_User", x => x.CreatedBy, "User", "UserId", "cm", onDelete: ReferentialAction.Restrict);
            });

        migrationBuilder.CreateTable(
            name: "EventType",
            schema: "audit",
            columns: table => new
            {
                EventTypeId = table.Column<int>(type: "int", nullable: false),
                Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                Description = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: true)
            },
            constraints: table => table.PrimaryKey("PK_EventType", x => x.EventTypeId));

        migrationBuilder.InsertData(
            schema: "audit", table: "EventType",
            columns: new[] { "EventTypeId", "Name", "Description" },
            values: new object[,]
            {
                { 1, "Create",               "Entity created" },
                { 2, "Update",               "Entity updated" },
                { 3, "Delete",               "Entity soft-deleted" },
                { 4, "ApprovalDecision",     "Approval status changed" },
                { 5, "StatusChange",         "Change request status updated" },
                { 6, "TaskUpdate",           "Task updated" },
                { 7, "AttachmentUploaded",   "File attached" }
            });

        migrationBuilder.CreateTable(
            name: "AuditEvent",
            schema: "audit",
            columns: table => new
            {
                AuditEventId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                EventTypeId = table.Column<int>(type: "int", nullable: false),
                ActorUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                ActorUpn = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                SchemaName = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                EntityName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                EntityId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                EntityNumber = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                Metadata = table.Column<string>(type: "nvarchar(max)", nullable: true),
                CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()")
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_AuditEvent", x => x.AuditEventId);
                table.ForeignKey("FK_AuditEvent_EventType", x => x.EventTypeId, "EventType", "EventTypeId", "audit", onDelete: ReferentialAction.Restrict);
                table.ForeignKey("FK_AuditEvent_User", x => x.ActorUserId, "User", "UserId", "cm", onDelete: ReferentialAction.Restrict);
            });

        // Section 5: INDEXES (performance + uniqueness)
        migrationBuilder.CreateIndex(name: "IX_User_Upn", schema: "cm", table: "User", column: "Upn", unique: true);

        migrationBuilder.CreateIndex(name: "IX_ChangeRequest_ChangeNumber", schema: "cm", table: "ChangeRequest", column: "ChangeNumber", unique: true);

        // FK indexes on ChangeRequest
        migrationBuilder.CreateIndex(name: "IX_ChangeRequest_ChangeTypeId", schema: "cm", table: "ChangeRequest", column: "ChangeTypeId");
        migrationBuilder.CreateIndex(name: "IX_ChangeRequest_PriorityId", schema: "cm", table: "ChangeRequest", column: "PriorityId");
        migrationBuilder.CreateIndex(name: "IX_ChangeRequest_StatusId", schema: "cm", table: "ChangeRequest", column: "StatusId");
        migrationBuilder.CreateIndex(name: "IX_ChangeRequest_RiskLevelId", schema: "cm", table: "ChangeRequest", column: "RiskLevelId");
        migrationBuilder.CreateIndex(name: "IX_ChangeRequest_ImpactLevelId", schema: "cm", table: "ChangeRequest", column: "ImpactLevelId");
        migrationBuilder.CreateIndex(name: "IX_ChangeRequest_ImpactTypeId", schema: "cm", table: "ChangeRequest", column: "ImpactTypeId");
        migrationBuilder.CreateIndex(name: "IX_ChangeRequest_RequestedByUserId", schema: "cm", table: "ChangeRequest", column: "RequestedByUserId");
        migrationBuilder.CreateIndex(name: "IX_ChangeRequest_AssignedToUserId", schema: "cm", table: "ChangeRequest", column: "AssignedToUserId");
        migrationBuilder.CreateIndex(name: "IX_ChangeRequest_SubmittedByUserId", schema: "cm", table: "ChangeRequest", column: "SubmittedByUserId");
        migrationBuilder.CreateIndex(name: "IX_ChangeRequest_CreatedBy", schema: "cm", table: "ChangeRequest", column: "CreatedBy");

        // Child table FK indexes
        migrationBuilder.CreateIndex(name: "IX_ChangeApprover_ChangeRequestId", schema: "cm", table: "ChangeApprover", column: "ChangeRequestId");
        migrationBuilder.CreateIndex(name: "IX_ChangeApprover_ApproverUserId", schema: "cm", table: "ChangeApprover", column: "ApproverUserId");
        migrationBuilder.CreateIndex(name: "IX_ChangeAttachment_ChangeRequestId", schema: "cm", table: "ChangeAttachment", column: "ChangeRequestId");
        migrationBuilder.CreateIndex(name: "IX_ChangeTask_ChangeRequestId", schema: "cm", table: "ChangeTask", column: "ChangeRequestId");
        migrationBuilder.CreateIndex(name: "IX_ChangeTask_AssignedToUserId", schema: "cm", table: "ChangeTask", column: "AssignedToUserId");
        migrationBuilder.CreateIndex(name: "IX_AuditEvent_EventTypeId", schema: "audit", table: "AuditEvent", column: "EventTypeId");

    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropSequence(name: "ChangeNumberSeq", schema: "cm");
        migrationBuilder.DropTable(name: "AuditEvent", schema: "audit");
        migrationBuilder.DropTable(name: "ChangeApprover", schema: "cm");
        migrationBuilder.DropTable(name: "ChangeAttachment", schema: "cm");
        migrationBuilder.DropTable(name: "ChangeTask", schema: "cm");
        migrationBuilder.DropTable(name: "ChangeTemplate", schema: "cm");
        migrationBuilder.DropTable(name: "ChangeRequest", schema: "cm");
        migrationBuilder.DropTable(name: "ImpactType", schema: "ref");
        migrationBuilder.DropTable(name: "ImpactLevel", schema: "ref");
        migrationBuilder.DropTable(name: "EventType", schema: "audit");
        migrationBuilder.DropTable(name: "ChangeType", schema: "ref");
        migrationBuilder.DropTable(name: "ChangePriority", schema: "ref");
        migrationBuilder.DropTable(name: "ChangeStatus", schema: "ref");
        migrationBuilder.DropTable(name: "RiskLevel", schema: "ref");
        migrationBuilder.DropTable(name: "User", schema: "cm");
    }
}
