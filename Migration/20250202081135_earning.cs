using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Pulse360.Migrations
{
    /// <inheritdoc />
    public partial class earning : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_Deduction_DepartmentId",
                table: "Deduction",
                column: "DepartmentId");

            migrationBuilder.CreateIndex(
                name: "IX_Deduction_DesignationId",
                table: "Deduction",
                column: "DesignationId");

            migrationBuilder.AddForeignKey(
                name: "FK_Deduction_Departments_DepartmentId",
                table: "Deduction",
                column: "DepartmentId",
                principalTable: "Departments",
                principalColumn: "DepartmentId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Deduction_Designations_DesignationId",
                table: "Deduction",
                column: "DesignationId",
                principalTable: "Designations",
                principalColumn: "DesignationId",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Deduction_Departments_DepartmentId",
                table: "Deduction");

            migrationBuilder.DropForeignKey(
                name: "FK_Deduction_Designations_DesignationId",
                table: "Deduction");

            migrationBuilder.DropIndex(
                name: "IX_Deduction_DepartmentId",
                table: "Deduction");

            migrationBuilder.DropIndex(
                name: "IX_Deduction_DesignationId",
                table: "Deduction");
        }
    }
}
