using ChangeManagement.Api.Data;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ChangeManagement.Api.Migrations;

[DbContext(typeof(ChangeManagementDbContext))]
[Migration("20260216120000_AddStructuredChangeRequestFields")]
public partial class AddStructuredChangeRequestFields : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<string>(
            name: "ImplementationSteps",
            schema: "cm",
            table: "ChangeRequest",
            type: "nvarchar(max)",
            nullable: true);

        migrationBuilder.AddColumn<string>(
            name: "BackoutPlan",
            schema: "cm",
            table: "ChangeRequest",
            type: "nvarchar(max)",
            nullable: true);

        migrationBuilder.AddColumn<string>(
            name: "ServiceSystem",
            schema: "cm",
            table: "ChangeRequest",
            type: "nvarchar(max)",
            nullable: true);

        migrationBuilder.AddColumn<string>(
            name: "Category",
            schema: "cm",
            table: "ChangeRequest",
            type: "nvarchar(max)",
            nullable: true);

        migrationBuilder.AddColumn<string>(
            name: "Environment",
            schema: "cm",
            table: "ChangeRequest",
            type: "nvarchar(max)",
            nullable: true);

        migrationBuilder.AddColumn<string>(
            name: "BusinessJustification",
            schema: "cm",
            table: "ChangeRequest",
            type: "nvarchar(max)",
            nullable: true);
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropColumn(name: "ImplementationSteps", schema: "cm", table: "ChangeRequest");
        migrationBuilder.DropColumn(name: "BackoutPlan", schema: "cm", table: "ChangeRequest");
        migrationBuilder.DropColumn(name: "ServiceSystem", schema: "cm", table: "ChangeRequest");
        migrationBuilder.DropColumn(name: "Category", schema: "cm", table: "ChangeRequest");
        migrationBuilder.DropColumn(name: "Environment", schema: "cm", table: "ChangeRequest");
        migrationBuilder.DropColumn(name: "BusinessJustification", schema: "cm", table: "ChangeRequest");
    }
}
