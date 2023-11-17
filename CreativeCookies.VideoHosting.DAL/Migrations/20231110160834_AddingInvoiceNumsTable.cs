using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CreativeCookies.VideoHosting.DAL.Migrations
{
    /// <inheritdoc />
    public partial class AddingInvoiceNumsTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "InvoiceNums",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(450)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InvoiceNums", x => x.Id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "InvoiceNums");
        }
    }
}
