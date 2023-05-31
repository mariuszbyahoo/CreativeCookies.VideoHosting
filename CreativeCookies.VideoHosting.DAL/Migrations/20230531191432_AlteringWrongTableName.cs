using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CreativeCookies.VideoHosting.DAL.Migrations
{
    /// <inheritdoc />
    public partial class AlteringWrongTableName : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_RefresTokens",
                table: "RefresTokens");

            migrationBuilder.RenameTable(
                name: "RefresTokens",
                newName: "RefreshTokens");

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAt",
                table: "RefreshTokens",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddPrimaryKey(
                name: "PK_RefreshTokens",
                table: "RefreshTokens",
                column: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_RefreshTokens",
                table: "RefreshTokens");

            migrationBuilder.DropColumn(
                name: "CreatedAt",
                table: "RefreshTokens");

            migrationBuilder.RenameTable(
                name: "RefreshTokens",
                newName: "RefresTokens");

            migrationBuilder.AddPrimaryKey(
                name: "PK_RefresTokens",
                table: "RefresTokens",
                column: "Id");
        }
    }
}
