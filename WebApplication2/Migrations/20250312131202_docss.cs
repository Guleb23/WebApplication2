using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WebApplication2.Migrations
{
    /// <inheritdoc />
    public partial class docss : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_DocumentModel_Users_UserId",
                table: "DocumentModel");

            migrationBuilder.DropPrimaryKey(
                name: "PK_DocumentModel",
                table: "DocumentModel");

            migrationBuilder.RenameTable(
                name: "DocumentModel",
                newName: "Documents");

            migrationBuilder.RenameIndex(
                name: "IX_DocumentModel_UserId",
                table: "Documents",
                newName: "IX_Documents_UserId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Documents",
                table: "Documents",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Documents_Users_UserId",
                table: "Documents",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Documents_Users_UserId",
                table: "Documents");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Documents",
                table: "Documents");

            migrationBuilder.RenameTable(
                name: "Documents",
                newName: "DocumentModel");

            migrationBuilder.RenameIndex(
                name: "IX_Documents_UserId",
                table: "DocumentModel",
                newName: "IX_DocumentModel_UserId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_DocumentModel",
                table: "DocumentModel",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_DocumentModel_Users_UserId",
                table: "DocumentModel",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
