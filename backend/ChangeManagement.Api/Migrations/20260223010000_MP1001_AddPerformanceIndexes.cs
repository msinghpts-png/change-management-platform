using ChangeManagement.Api.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ChangeManagement.Api.Migrations;

[DbContext(typeof(ChangeManagementDbContext))]
[Migration("20260223010000_MP1001_AddPerformanceIndexes")]
public partial class MP1001_AddPerformanceIndexes : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        // Indexes are now created in 20260223000000_MP1001_DbRebuild to avoid duplicate index creation
        // when applying migrations on a clean database.
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        // No-op: Up does not create indexes.
    }
}
