using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ClaimSystem.Migrations
{
    /// <inheritdoc />
    public partial class AddUserTableAndRelationship : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // First, ensure the UserId column exists in Lecturers table
            // If it doesn't exist, add it
            migrationBuilder.AddColumn<int>(
                name: "UserId",
                table: "Lecturers",
                type: "int",
                nullable: false,
                defaultValue: 0);

            // Create the foreign key relationship
            migrationBuilder.AddForeignKey(
                name: "FK_Lecturers_Users_UserId",
                table: "Lecturers",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "UserId",
                onDelete: ReferentialAction.Cascade);

            // Create an index for better performance
            migrationBuilder.CreateIndex(
                name: "IX_Lecturers_UserId",
                table: "Lecturers",
                column: "UserId",
                unique: true); // One-to-one relationship
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Lecturers_Users_UserId",
                table: "Lecturers");

            migrationBuilder.DropIndex(
                name: "IX_Lecturers_UserId",
                table: "Lecturers");

            migrationBuilder.DropColumn(
                name: "UserId",
                table: "Lecturers");
        }
    }
}
