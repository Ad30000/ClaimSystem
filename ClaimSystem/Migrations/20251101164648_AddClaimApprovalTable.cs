using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ClaimSystem.Migrations
{
    public partial class AddClaimApprovalTable : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ClaimApproval",
                columns: table => new
                {
                    ClaimApprovalId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ClaimId = table.Column<int>(type: "int", nullable: false),
                    UserId = table.Column<int>(type: "int", nullable: false),
                    ApprovalDate = table.Column<DateTime>(type: "datetime", nullable: false, defaultValueSql: "(getdate())"),
                    Status = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ClaimApproval", x => x.ClaimApprovalId);
                    table.ForeignKey(
                        name: "FK_ClaimApproval_Claim",
                        column: x => x.ClaimId,
                        principalTable: "Claim",
                        principalColumn: "ClaimId",
                        onDelete: ReferentialAction.NoAction); // Changed from Cascade to NoAction
                    table.ForeignKey(
                        name: "FK_ClaimApproval_User",
                        column: x => x.UserId,
                        principalTable: "User",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.NoAction); // Changed from Cascade to NoAction
                });

            migrationBuilder.CreateIndex(
                name: "IX_ClaimApproval_ClaimId",
                table: "ClaimApproval",
                column: "ClaimId");

            migrationBuilder.CreateIndex(
                name: "IX_ClaimApproval_UserId",
                table: "ClaimApproval",
                column: "UserId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ClaimApproval");
        }
    }
}