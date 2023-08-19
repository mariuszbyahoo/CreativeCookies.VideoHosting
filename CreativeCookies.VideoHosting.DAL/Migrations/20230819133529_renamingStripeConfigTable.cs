using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CreativeCookies.VideoHosting.DAL.Migrations
{
    /// <inheritdoc />
    public partial class renamingStripeConfigTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_StripeAccountRecords",
                table: "StripeAccountRecords");

            migrationBuilder.RenameTable(
                name: "StripeAccountRecords",
                newName: "StripeConfig");

            migrationBuilder.AddPrimaryKey(
                name: "PK_StripeConfig",
                table: "StripeConfig",
                column: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_StripeConfig",
                table: "StripeConfig");

            migrationBuilder.RenameTable(
                name: "StripeConfig",
                newName: "StripeAccountRecords");

            migrationBuilder.AddPrimaryKey(
                name: "PK_StripeAccountRecords",
                table: "StripeAccountRecords",
                column: "Id");
        }
    }
}
