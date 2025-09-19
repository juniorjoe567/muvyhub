using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MuvyHub.Migrations
{
    /// <inheritdoc />
    public partial class AddOriginalFileNameToUploads : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "FileName",
                table: "UploadJobs",
                newName: "WasabiKey");

            migrationBuilder.AddColumn<string>(
                name: "OriginalFileName",
                table: "UploadJobs",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "OriginalFileName",
                table: "UploadJobs");

            migrationBuilder.RenameColumn(
                name: "WasabiKey",
                table: "UploadJobs",
                newName: "FileName");
        }
    }
}
