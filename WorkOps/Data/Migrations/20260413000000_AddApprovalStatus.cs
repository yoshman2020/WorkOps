using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WorkOps.Migrations
{
    /// <inheritdoc />
    public partial class AddApprovalStatus : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "MApprovalStatusId",
                table: "TReport",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "MApprovalStatus",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
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
                    table.PrimaryKey("PK_MApprovalStatus", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MApprovalStatus_AspNetUsers_CreatedBy",
                        column: x => x.CreatedBy,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_MApprovalStatus_AspNetUsers_UpdatedBy",
                        column: x => x.UpdatedBy,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "TAttendanceStatus",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    UserId = table.Column<string>(type: "TEXT", nullable: false),
                    Year = table.Column<int>(type: "INTEGER", nullable: false),
                    Month = table.Column<int>(type: "INTEGER", nullable: false),
                    MApprovalStatusId = table.Column<int>(type: "INTEGER", nullable: true),
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
                    table.PrimaryKey("PK_TAttendanceStatus", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TAttendanceStatus_AspNetUsers_CreatedBy",
                        column: x => x.CreatedBy,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_TAttendanceStatus_AspNetUsers_UpdatedBy",
                        column: x => x.UpdatedBy,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_TAttendanceStatus_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_TAttendanceStatus_MApprovalStatus_MApprovalStatusId",
                        column: x => x.MApprovalStatusId,
                        principalTable: "MApprovalStatus",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_TReport_MApprovalStatusId",
                table: "TReport",
                column: "MApprovalStatusId");

            migrationBuilder.CreateIndex(
                name: "IX_MApprovalStatus_CreatedBy",
                table: "MApprovalStatus",
                column: "CreatedBy");

            migrationBuilder.CreateIndex(
                name: "IX_MApprovalStatus_UpdatedBy",
                table: "MApprovalStatus",
                column: "UpdatedBy");

            migrationBuilder.CreateIndex(
                name: "IX_TAttendanceStatus_CreatedBy",
                table: "TAttendanceStatus",
                column: "CreatedBy");

            migrationBuilder.CreateIndex(
                name: "IX_TAttendanceStatus_MApprovalStatusId",
                table: "TAttendanceStatus",
                column: "MApprovalStatusId");

            migrationBuilder.CreateIndex(
                name: "IX_TAttendanceStatus_UpdatedBy",
                table: "TAttendanceStatus",
                column: "UpdatedBy");

            migrationBuilder.CreateIndex(
                name: "IX_TAttendanceStatus_UserId",
                table: "TAttendanceStatus",
                column: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_TReport_MApprovalStatus_MApprovalStatusId",
                table: "TReport",
                column: "MApprovalStatusId",
                principalTable: "MApprovalStatus",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_TReport_MApprovalStatus_MApprovalStatusId",
                table: "TReport");

            migrationBuilder.DropTable(
                name: "TAttendanceStatus");

            migrationBuilder.DropTable(
                name: "MApprovalStatus");

            migrationBuilder.DropIndex(
                name: "IX_TReport_MApprovalStatusId",
                table: "TReport");

            migrationBuilder.DropColumn(
                name: "MApprovalStatusId",
                table: "TReport");
        }
    }
}
