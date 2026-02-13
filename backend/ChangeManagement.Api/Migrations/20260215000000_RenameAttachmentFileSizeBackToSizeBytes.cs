using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ChangeManagement.Api.Migrations
{
    public partial class RenameAttachmentFileSizeBackToSizeBytes : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "FileSize",
                table: "ChangeAttachments",
                newName: "SizeBytes");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "SizeBytes",
                table: "ChangeAttachments",
                newName: "FileSize");
        }
    }
}
