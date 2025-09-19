using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MuvyHub.Migrations
{
    /// <inheritdoc />
    public partial class AddViewCountToUploadJobs : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "ViewCount",
                table: "UploadJobs",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ViewCount",
                table: "UploadJobs");
        }
    }
}
