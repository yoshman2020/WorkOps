using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WorkOps.Migrations
{
    /// <inheritdoc />
    public partial class AddPrevReminderEnabled : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsPrevReminderEnabled",
                table: "AspNetUsers",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsPrevReminderEnabled",
                table: "AspNetUsers");
        }
    }
}
