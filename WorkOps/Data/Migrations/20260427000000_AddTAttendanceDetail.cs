using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WorkOps.Migrations
{
    /// <inheritdoc />
    public partial class AddTAttendanceDetail : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "TAttendanceDetail",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    TAttendanceId = table.Column<int>(type: "INTEGER", nullable: false),
                    WorkDetailAm = table.Column<string>(type: "TEXT", nullable: true),
                    WorkDetailPm = table.Column<string>(type: "TEXT", nullable: true),
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
                    table.PrimaryKey("PK_TAttendanceDetail", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TAttendanceDetail_AspNetUsers_CreatedBy",
                        column: x => x.CreatedBy,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_TAttendanceDetail_AspNetUsers_UpdatedBy",
                        column: x => x.UpdatedBy,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_TAttendanceDetail_TAttendance_TAttendanceId",
                        column: x => x.TAttendanceId,
                        principalTable: "TAttendance",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_TAttendanceDetail_CreatedBy",
                table: "TAttendanceDetail",
                column: "CreatedBy");

            migrationBuilder.CreateIndex(
                name: "IX_TAttendanceDetail_TAttendanceId",
                table: "TAttendanceDetail",
                column: "TAttendanceId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_TAttendanceDetail_UpdatedBy",
                table: "TAttendanceDetail",
                column: "UpdatedBy");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "TAttendanceDetail");
        }
    }
}
