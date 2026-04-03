using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WorkOps.Migrations
{
    /// <inheritdoc />
    public partial class AddTPaidLeave : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "TPaidLeave",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    UserId = table.Column<string>(type: "TEXT", nullable: false),
                    GrantedDays = table.Column<int>(type: "INTEGER", nullable: false),
                    GrantedDate = table.Column<DateOnly>(type: "TEXT", nullable: false),
                    ExpiredDate = table.Column<DateOnly>(type: "TEXT", nullable: false),
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
                    table.PrimaryKey("PK_TPaidLeave", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TPaidLeave_AspNetUsers_CreatedBy",
                        column: x => x.CreatedBy,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_TPaidLeave_AspNetUsers_UpdatedBy",
                        column: x => x.UpdatedBy,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_TPaidLeave_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_TPaidLeave_CreatedBy",
                table: "TPaidLeave",
                column: "CreatedBy");

            migrationBuilder.CreateIndex(
                name: "IX_TPaidLeave_UpdatedBy",
                table: "TPaidLeave",
                column: "UpdatedBy");

            migrationBuilder.CreateIndex(
                name: "IX_TPaidLeave_UserId",
                table: "TPaidLeave",
                column: "UserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "TPaidLeave");
        }
    }
}
