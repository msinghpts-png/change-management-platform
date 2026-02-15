// Replaced onDelete action for FK_ChangeTask_ChangeStatus_StatusId

// ... [rest of the file contents above line 215] ...

public void Up()
{
    // other migration code...
    migrationBuilder.AddForeignKey(
        name: "FK_ChangeTask_ChangeStatus_StatusId",
        table: "ChangeTask",
        column: "StatusId",
        principalTable: "ChangeStatus",
        principalColumn: "Id",
        onDelete: ReferentialAction.Restrict); // Changed from Cascade to Restrict
}

// ... [rest of the file contents below line 215] ...