using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ChangeManagement.Api.Migrations
{
    public partial class InitialCreateWithAttachments : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ChangeRequests",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Title = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    Priority = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    RiskLevel = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    PlannedStart = table.Column<DateTime>(type: "datetime2", nullable: true),
                    PlannedEnd = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ChangeRequests", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ChangeApprovals",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ChangeRequestId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Approver = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    Comment = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    DecisionAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ChangeApprovals", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ChangeApprovals_ChangeRequests_ChangeRequestId",
                        column: x => x.ChangeRequestId,
                        principalTable: "ChangeRequests",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ChangeAttachments",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ChangeRequestId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    FileName = table.Column<string>(type: "nvarchar(260)", maxLength: 260, nullable: false),
                    ContentType = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false),
                    SizeBytes = table.Column<long>(type: "bigint", nullable: false),
                    StoragePath = table.Column<string>(type: "nvarchar(1024)", maxLength: 1024, nullable: false),
                    UploadedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ChangeAttachments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ChangeAttachments_ChangeRequests_ChangeRequestId",
                        column: x => x.ChangeRequestId,
                        principalTable: "ChangeRequests",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ChangeApprovals_ChangeRequestId",
                table: "ChangeApprovals",
                column: "ChangeRequestId");

            migrationBuilder.CreateIndex(
                name: "IX_ChangeAttachments_ChangeRequestId_FileName_UploadedAt",
                table: "ChangeAttachments",
                columns: new[] { "ChangeRequestId", "FileName", "UploadedAt" });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ChangeApprovals");

            migrationBuilder.DropTable(
                name: "ChangeAttachments");

            migrationBuilder.DropTable(
                name: "ChangeRequests");
        }
    }
}
