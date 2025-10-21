using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ClaimSystem.Migrations
{
    /// <inheritdoc />
    public partial class InitialMigration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "User",
                columns: table => new
                {
                    UserId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Username = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    PasswordHash = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    Email = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    Role = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__User__1788CC4CB3B45EFA", x => x.UserId);
                });

            migrationBuilder.CreateTable(
                name: "Lecturer",
                columns: table => new
                {
                    LecturerId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<int>(type: "int", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Department = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    PhoneNumber = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: true),
                    Address = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__Lecturer__5A78B93D23C3DCC3", x => x.LecturerId);
                    table.ForeignKey(
                        name: "FK_Lecturer_User",
                        column: x => x.UserId,
                        principalTable: "User",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Claim",
                columns: table => new
                {
                    ClaimId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ClaimNumber = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    LastUpdatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    LecturerId = table.Column<int>(type: "int", nullable: false),
                    MonthClaim = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    HoursWorked = table.Column<decimal>(type: "decimal(10,2)", nullable: false),
                    AmountPerHour = table.Column<decimal>(type: "decimal(10,2)", nullable: false),
                    TotalAmount = table.Column<decimal>(type: "decimal(21,4)", nullable: true, computedColumnSql: "([HoursWorked]*[AmountPerHour])", stored: true),
                    Status = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false, defaultValue: "Pending"),
                    DateSubmitted = table.Column<DateTime>(type: "datetime", nullable: false, defaultValueSql: "(getdate())"),
                    AdditionalNotes = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__Claim__EF2E139BC542D98F", x => x.ClaimId);
                    table.ForeignKey(
                        name: "FK_Claim_Lecturer",
                        column: x => x.LecturerId,
                        principalTable: "Lecturer",
                        principalColumn: "LecturerId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "SupportingDocument",
                columns: table => new
                {
                    SupportingDocumentId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ClaimId = table.Column<int>(type: "int", nullable: false),
                    FileContent = table.Column<byte[]>(type: "varbinary(max)", nullable: false),
                    FileName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    FileSize = table.Column<long>(type: "bigint", nullable: false),
                    ContentType = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false, defaultValue: "application/octet-stream"),
                    FileUploadDate = table.Column<DateTime>(type: "datetime", nullable: false, defaultValueSql: "(getdate())")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__Supporti__9DD3C21C647019FF", x => x.SupportingDocumentId);
                    table.ForeignKey(
                        name: "FK_SupportingDocument_Claim",
                        column: x => x.ClaimId,
                        principalTable: "Claim",
                        principalColumn: "ClaimId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Claim_LecturerId",
                table: "Claim",
                column: "LecturerId");

            migrationBuilder.CreateIndex(
                name: "UQ__Lecturer__1788CC4DC72B8E1D",
                table: "Lecturer",
                column: "UserId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_SupportingDocument_ClaimId",
                table: "SupportingDocument",
                column: "ClaimId");

            migrationBuilder.CreateIndex(
                name: "UQ__User__536C85E47BB91CFB",
                table: "User",
                column: "Username",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "UQ__User__A9D10534F9976CCD",
                table: "User",
                column: "Email",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "SupportingDocument");

            migrationBuilder.DropTable(
                name: "Claim");

            migrationBuilder.DropTable(
                name: "Lecturer");

            migrationBuilder.DropTable(
                name: "User");
        }
    }
}
