using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Hyre.API.Migrations
{
    /// <inheritdoc />
    public partial class AlterJobCreatedByTypeToString : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Jobs_Users_CreatedBy",
                table: "Jobs");

            migrationBuilder.AlterColumn<string>(
                name: "CreatedBy",
                table: "Jobs",
                type: "nvarchar(450)",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AddForeignKey(
                name: "FK_Jobs_AspNetUsers_CreatedBy",
                table: "Jobs",
                column: "CreatedBy",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Jobs_AspNetUsers_CreatedBy",
                table: "Jobs");

            migrationBuilder.AlterColumn<int>(
                name: "CreatedBy",
                table: "Jobs",
                type: "int",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)");

            migrationBuilder.AddForeignKey(
                name: "FK_Jobs_Users_CreatedBy",
                table: "Jobs",
                column: "CreatedBy",
                principalTable: "Users",
                principalColumn: "UserID",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
