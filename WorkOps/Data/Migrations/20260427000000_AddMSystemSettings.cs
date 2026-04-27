using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WorkOps.Migrations
{
    /// <inheritdoc />
    public partial class AddMSystemSettings : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "MSystemSettings",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    IsSendSubmittedStatusMail = table.Column<bool>(type: "INTEGER", nullable: false),
                    Name = table.Column<string>(type: "TEXT", nullable: false),
                    Remarks = table.Column<string>(type: "TEXT", nullable: false),
                    IsDeleted = table.Column<bool>(type: "INTEGER", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    CreatedBy = table.Column<string>(type: "TEXT", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    UpdatedBy = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MSystemSettings", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MSystemSettings_AspNetUsers_CreatedBy",
                        column: x => x.CreatedBy,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_MSystemSettings_AspNetUsers_UpdatedBy",
                        column: x => x.UpdatedBy,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_TReport_UserId",
                table: "TReport",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_MSystemSettings_CreatedBy",
                table: "MSystemSettings",
                column: "CreatedBy");

            migrationBuilder.CreateIndex(
                name: "IX_MSystemSettings_UpdatedBy",
                table: "MSystemSettings",
                column: "UpdatedBy");

            migrationBuilder.AddForeignKey(
                name: "FK_TReport_AspNetUsers_UserId",
                table: "TReport",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_TReport_AspNetUsers_UserId",
                table: "TReport");

            migrationBuilder.DropTable(
                name: "MSystemSettings");

            migrationBuilder.DropIndex(
                name: "IX_TReport_UserId",
                table: "TReport");
        }
    }
}
