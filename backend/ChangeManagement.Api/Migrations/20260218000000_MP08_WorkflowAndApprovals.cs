using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ChangeManagement.Api.Migrations;

[Migration("20260218000000_MP08_WorkflowAndApprovals")]
public partial class MP08_WorkflowAndApprovals : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<bool>(name: "ApprovalRequired", schema: "cm", table: "ChangeRequest", type: "bit", nullable: false, defaultValue: false);
        migrationBuilder.AddColumn<string>(name: "ApprovalStrategy", schema: "cm", table: "ChangeRequest", type: "nvarchar(20)", maxLength: 20, nullable: false, defaultValue: "Any");
        migrationBuilder.AddColumn<Guid>(name: "ApprovalRequesterUserId", schema: "cm", table: "ChangeRequest", type: "uniqueidentifier", nullable: true);
        migrationBuilder.AddColumn<DateTime>(name: "SubmittedAt", schema: "cm", table: "ChangeRequest", type: "datetime2", nullable: true);
        migrationBuilder.AddColumn<Guid>(name: "SubmittedByUserId", schema: "cm", table: "ChangeRequest", type: "uniqueidentifier", nullable: true);
        migrationBuilder.AddColumn<string>(name: "ImplementationGroup", schema: "cm", table: "ChangeRequest", type: "nvarchar(200)", maxLength: 200, nullable: true);
        migrationBuilder.AddColumn<int>(name: "ImpactLevelId", schema: "cm", table: "ChangeRequest", type: "int", nullable: true);
        migrationBuilder.AddColumn<DateTime>(name: "DeletedAt", schema: "cm", table: "ChangeRequest", type: "datetime2", nullable: true);
        migrationBuilder.AddColumn<Guid>(name: "DeletedByUserId", schema: "cm", table: "ChangeRequest", type: "uniqueidentifier", nullable: true);
        migrationBuilder.AddColumn<string>(name: "DeletedReason", schema: "cm", table: "ChangeRequest", type: "nvarchar(400)", maxLength: 400, nullable: true);

        migrationBuilder.CreateTable(
            name: "ChangeApprover",
            schema: "cm",
            columns: table => new
            {
                ChangeApproverId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                ChangeRequestId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                ApproverUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_ChangeApprover", x => x.ChangeApproverId);
                table.ForeignKey(name: "FK_ChangeApprover_ChangeRequest_ChangeRequestId", column: x => x.ChangeRequestId, principalSchema: "cm", principalTable: "ChangeRequest", principalColumn: "ChangeRequestId", onDelete: ReferentialAction.Cascade);
                table.ForeignKey(name: "FK_ChangeApprover_User_ApproverUserId", column: x => x.ApproverUserId, principalSchema: "cm", principalTable: "User", principalColumn: "UserId", onDelete: ReferentialAction.Restrict);
            });

        migrationBuilder.CreateIndex(name: "IX_ChangeRequest_ApprovalRequesterUserId", schema: "cm", table: "ChangeRequest", column: "ApprovalRequesterUserId");
        migrationBuilder.CreateIndex(name: "IX_ChangeRequest_SubmittedByUserId", schema: "cm", table: "ChangeRequest", column: "SubmittedByUserId");
        migrationBuilder.CreateIndex(name: "IX_ChangeRequest_DeletedByUserId", schema: "cm", table: "ChangeRequest", column: "DeletedByUserId");
        migrationBuilder.CreateIndex(name: "IX_ChangeRequest_ImpactLevelId", schema: "cm", table: "ChangeRequest", column: "ImpactLevelId");

        migrationBuilder.CreateIndex(name: "IX_ChangeApprover_ApproverUserId", schema: "cm", table: "ChangeApprover", column: "ApproverUserId");
        migrationBuilder.CreateIndex(name: "IX_ChangeApprover_ChangeRequestId_ApproverUserId", schema: "cm", table: "ChangeApprover", columns: new[] { "ChangeRequestId", "ApproverUserId" }, unique: true);

        migrationBuilder.CreateIndex(name: "IX_ChangeApproval_ChangeRequestId_ApproverUserId", schema: "cm", table: "ChangeApproval", columns: new[] { "ChangeRequestId", "ApproverUserId" }, unique: true);

        migrationBuilder.AddForeignKey(name: "FK_ChangeRequest_User_ApprovalRequesterUserId", schema: "cm", table: "ChangeRequest", column: "ApprovalRequesterUserId", principalSchema: "cm", principalTable: "User", principalColumn: "UserId", onDelete: ReferentialAction.Restrict);
        migrationBuilder.AddForeignKey(name: "FK_ChangeRequest_User_SubmittedByUserId", schema: "cm", table: "ChangeRequest", column: "SubmittedByUserId", principalSchema: "cm", principalTable: "User", principalColumn: "UserId", onDelete: ReferentialAction.Restrict);
        migrationBuilder.AddForeignKey(name: "FK_ChangeRequest_User_DeletedByUserId", schema: "cm", table: "ChangeRequest", column: "DeletedByUserId", principalSchema: "cm", principalTable: "User", principalColumn: "UserId", onDelete: ReferentialAction.Restrict);
        migrationBuilder.AddForeignKey(name: "FK_ChangeRequest_RiskLevel_ImpactLevelId", schema: "cm", table: "ChangeRequest", column: "ImpactLevelId", principalSchema: "ref", principalTable: "RiskLevel", principalColumn: "RiskLevelId", onDelete: ReferentialAction.Restrict);

        migrationBuilder.Sql(@"
MERGE [ref].[ChangeStatus] AS t
USING (VALUES
(1,'Draft',0),(2,'Submitted',0),(3,'PendingApproval',0),(4,'Approved',0),(5,'Rejected',1),
(6,'Scheduled',0),(7,'InImplementation',0),(8,'Completed',1),(9,'Closed',1),(10,'Cancelled',1)
) AS s(StatusId,Name,IsTerminal)
ON t.StatusId = s.StatusId
WHEN MATCHED THEN UPDATE SET t.Name=s.Name, t.IsTerminal=s.IsTerminal
WHEN NOT MATCHED THEN INSERT(StatusId,Name,IsTerminal) VALUES(s.StatusId,s.Name,s.IsTerminal);
");
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropForeignKey(name: "FK_ChangeRequest_User_ApprovalRequesterUserId", schema: "cm", table: "ChangeRequest");
        migrationBuilder.DropForeignKey(name: "FK_ChangeRequest_User_SubmittedByUserId", schema: "cm", table: "ChangeRequest");
        migrationBuilder.DropForeignKey(name: "FK_ChangeRequest_User_DeletedByUserId", schema: "cm", table: "ChangeRequest");
        migrationBuilder.DropForeignKey(name: "FK_ChangeRequest_RiskLevel_ImpactLevelId", schema: "cm", table: "ChangeRequest");
        migrationBuilder.DropTable(name: "ChangeApprover", schema: "cm");
        migrationBuilder.DropIndex(name: "IX_ChangeApproval_ChangeRequestId_ApproverUserId", schema: "cm", table: "ChangeApproval");
        migrationBuilder.DropIndex(name: "IX_ChangeRequest_ApprovalRequesterUserId", schema: "cm", table: "ChangeRequest");
        migrationBuilder.DropIndex(name: "IX_ChangeRequest_SubmittedByUserId", schema: "cm", table: "ChangeRequest");
        migrationBuilder.DropIndex(name: "IX_ChangeRequest_DeletedByUserId", schema: "cm", table: "ChangeRequest");
        migrationBuilder.DropIndex(name: "IX_ChangeRequest_ImpactLevelId", schema: "cm", table: "ChangeRequest");
        migrationBuilder.DropColumn(name: "ApprovalRequired", schema: "cm", table: "ChangeRequest");
        migrationBuilder.DropColumn(name: "ApprovalStrategy", schema: "cm", table: "ChangeRequest");
        migrationBuilder.DropColumn(name: "ApprovalRequesterUserId", schema: "cm", table: "ChangeRequest");
        migrationBuilder.DropColumn(name: "SubmittedAt", schema: "cm", table: "ChangeRequest");
        migrationBuilder.DropColumn(name: "SubmittedByUserId", schema: "cm", table: "ChangeRequest");
        migrationBuilder.DropColumn(name: "ImplementationGroup", schema: "cm", table: "ChangeRequest");
        migrationBuilder.DropColumn(name: "ImpactLevelId", schema: "cm", table: "ChangeRequest");
        migrationBuilder.DropColumn(name: "DeletedAt", schema: "cm", table: "ChangeRequest");
        migrationBuilder.DropColumn(name: "DeletedByUserId", schema: "cm", table: "ChangeRequest");
        migrationBuilder.DropColumn(name: "DeletedReason", schema: "cm", table: "ChangeRequest");
    }
}
