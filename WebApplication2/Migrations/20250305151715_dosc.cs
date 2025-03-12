using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace WebApplication2.Migrations
{
    /// <inheritdoc />
    public partial class dosc : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "GetDocsSposob",
                table: "Users");

            migrationBuilder.AddColumn<int>(
                name: "GetDocsSposobId",
                table: "Users",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "GetDocsSposobModels",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GetDocsSposobModels", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Users_GetDocsSposobId",
                table: "Users",
                column: "GetDocsSposobId");

            migrationBuilder.AddForeignKey(
                name: "FK_Users_GetDocsSposobModels_GetDocsSposobId",
                table: "Users",
                column: "GetDocsSposobId",
                principalTable: "GetDocsSposobModels",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Users_GetDocsSposobModels_GetDocsSposobId",
                table: "Users");

            migrationBuilder.DropTable(
                name: "GetDocsSposobModels");

            migrationBuilder.DropIndex(
                name: "IX_Users_GetDocsSposobId",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "GetDocsSposobId",
                table: "Users");

            migrationBuilder.AddColumn<string>(
                name: "GetDocsSposob",
                table: "Users",
                type: "text",
                nullable: false,
                defaultValue: "");
        }
    }
}
