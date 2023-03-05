using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CreativeCookies.VideoHosting.EfCore.Migrations
{
    /// <inheritdoc />
    public partial class third : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_VideoSegment_Video_VideoId",
                table: "VideoSegment");

            migrationBuilder.DropPrimaryKey(
                name: "PK_VideoSegment",
                table: "VideoSegment");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Video",
                table: "Video");

            migrationBuilder.RenameTable(
                name: "VideoSegment",
                newName: "VideoSegments");

            migrationBuilder.RenameTable(
                name: "Video",
                newName: "Videos");

            migrationBuilder.RenameIndex(
                name: "IX_VideoSegment_VideoId",
                table: "VideoSegments",
                newName: "IX_VideoSegments_VideoId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_VideoSegments",
                table: "VideoSegments",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Videos",
                table: "Videos",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_VideoSegments_Videos_VideoId",
                table: "VideoSegments",
                column: "VideoId",
                principalTable: "Videos",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_VideoSegments_Videos_VideoId",
                table: "VideoSegments");

            migrationBuilder.DropPrimaryKey(
                name: "PK_VideoSegments",
                table: "VideoSegments");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Videos",
                table: "Videos");

            migrationBuilder.RenameTable(
                name: "VideoSegments",
                newName: "VideoSegment");

            migrationBuilder.RenameTable(
                name: "Videos",
                newName: "Video");

            migrationBuilder.RenameIndex(
                name: "IX_VideoSegments_VideoId",
                table: "VideoSegment",
                newName: "IX_VideoSegment_VideoId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_VideoSegment",
                table: "VideoSegment",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Video",
                table: "Video",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_VideoSegment_Video_VideoId",
                table: "VideoSegment",
                column: "VideoId",
                principalTable: "Video",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
