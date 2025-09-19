using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MuvyHub.Migrations
{
    /// <inheritdoc />
    public partial class UpdateExpiryDates : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "ExpiryDate",
                table: "AspNetUsers",
                type: "datetime2",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ExpiryDate",
                table: "AspNetUsers");
        }
    }
}
