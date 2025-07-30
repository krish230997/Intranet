using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Pulse360.Migrations
{
    /// <inheritdoc />
    public partial class earningfk : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Deduction_DeductionType_DeductionTypeId",
                table: "Deduction");

            migrationBuilder.RenameColumn(
                name: "DeductionTypeId",
                table: "Deduction",
                newName: "Id");

            migrationBuilder.RenameIndex(
                name: "IX_Deduction_DeductionTypeId",
                table: "Deduction",
                newName: "IX_Deduction_Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Deduction_DeductionType_Id",
                table: "Deduction",
                column: "Id",
                principalTable: "DeductionType",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Deduction_DeductionType_Id",
                table: "Deduction");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "Deduction",
                newName: "DeductionTypeId");

            migrationBuilder.RenameIndex(
                name: "IX_Deduction_Id",
                table: "Deduction",
                newName: "IX_Deduction_DeductionTypeId");

            migrationBuilder.AddForeignKey(
                name: "FK_Deduction_DeductionType_DeductionTypeId",
                table: "Deduction",
                column: "DeductionTypeId",
                principalTable: "DeductionType",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
