using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Pulse360.Migrations
{
    /// <inheritdoc />
    public partial class ganesh : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Grade",
                table: "EducationDetails");

            migrationBuilder.DropColumn(
                name: "YearOfPassing",
                table: "EducationDetails");

            migrationBuilder.AddColumn<string>(
                name: "CompanyName",
                table: "Experience",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<DateTime>(
                name: "enddate",
                table: "EducationDetails",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "startdate",
                table: "EducationDetails",
                type: "datetime2",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CompanyName",
                table: "Experience");

            migrationBuilder.DropColumn(
                name: "enddate",
                table: "EducationDetails");

            migrationBuilder.DropColumn(
                name: "startdate",
                table: "EducationDetails");

            migrationBuilder.AddColumn<string>(
                name: "Grade",
                table: "EducationDetails",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "YearOfPassing",
                table: "EducationDetails",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }
    }
}
