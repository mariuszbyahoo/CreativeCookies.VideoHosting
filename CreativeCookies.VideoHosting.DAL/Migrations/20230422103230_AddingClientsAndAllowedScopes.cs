using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CreativeCookies.VideoHosting.DAL.Migrations
{
    /// <inheritdoc />
    public partial class AddingClientsAndAllowedScopes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "OAuthClients",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false, defaultValueSql: "newsequentialid()"),
                    ClientId = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ClientSecret = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    RedirectUri = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OAuthClients", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AllowedScopes",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false, defaultValueSql: "newsequentialid()"),
                    Scope = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    OAuthClientId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AllowedScopes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AllowedScopes_OAuthClients_OAuthClientId",
                        column: x => x.OAuthClientId,
                        principalTable: "OAuthClients",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AllowedScopes_OAuthClientId",
                table: "AllowedScopes",
                column: "OAuthClientId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AllowedScopes");

            migrationBuilder.DropTable(
                name: "OAuthClients");
        }
    }
}
