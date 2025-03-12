using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace WebApplication2.Migrations
{
    /// <inheritdoc />
    public partial class payments : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 1);

            migrationBuilder.DropColumn(
                name: "PaySposob",
                table: "Users");

            migrationBuilder.AddColumn<int>(
                name: "PaymentMethodId",
                table: "Users",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "PaymentMethods",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PaymentMethods", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Users_PaymentMethodId",
                table: "Users",
                column: "PaymentMethodId");

            migrationBuilder.AddForeignKey(
                name: "FK_Users_PaymentMethods_PaymentMethodId",
                table: "Users",
                column: "PaymentMethodId",
                principalTable: "PaymentMethods",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Users_PaymentMethods_PaymentMethodId",
                table: "Users");

            migrationBuilder.DropTable(
                name: "PaymentMethods");

            migrationBuilder.DropIndex(
                name: "IX_Users_PaymentMethodId",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "PaymentMethodId",
                table: "Users");

            migrationBuilder.AddColumn<string>(
                name: "PaySposob",
                table: "Users",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.InsertData(
                table: "Users",
                columns: new[] { "Id", "Email", "FirstName", "GetDocsSposob", "LastName", "Password", "PaySposob", "Phone" },
                values: new object[] { 1, null, "Laptop", "", "123", "", "", "" });
        }
    }
}
