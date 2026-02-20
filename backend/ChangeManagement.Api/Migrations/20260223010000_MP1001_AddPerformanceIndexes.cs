using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ChangeManagement.Api.Migrations
{
    [DbContext(typeof(ChangeManagementDbContext))]
    [Migration("20260223010000_MP1001_AddPerformanceIndexes")]
    public partial class MP1001_AddPerformanceIndexes : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_ChangeRequest_StatusId",
                schema: "cm",
                table: "ChangeRequest",
                column: "StatusId");

            migrationBuilder.CreateIndex(
                name: "IX_ChangeRequest_ChangeNumber",
                schema: "cm",
                table: "ChangeRequest",
                column: "ChangeNumber",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ChangeRequest_RequestedByUserId",
                schema: "cm",
                table: "ChangeRequest",
                column: "RequestedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_ChangeApprover_ChangeRequestId",
                schema: "cm",
                table: "ChangeApprover",
                column: "ChangeRequestId");

            migrationBuilder.CreateIndex(
                name: "IX_ChangeAttachment_ChangeRequestId",
                schema: "cm",
                table: "ChangeAttachment",
                column: "ChangeRequestId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex("IX_ChangeRequest_StatusId", "cm", "ChangeRequest");
            migrationBuilder.DropIndex("IX_ChangeRequest_ChangeNumber", "cm", "ChangeRequest");
            migrationBuilder.DropIndex("IX_ChangeRequest_RequestedByUserId", "cm", "ChangeRequest");
            migrationBuilder.DropIndex("IX_ChangeApprover_ChangeRequestId", "cm", "ChangeApprover");
            migrationBuilder.DropIndex("IX_ChangeAttachment_ChangeRequestId", "cm", "ChangeAttachment");
        }
    }
}
