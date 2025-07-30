using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Pulse360.Migrations
{
    /// <inheritdoc />
    public partial class payslipdearning : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Earning_EarningType_EarntypeId",
                table: "Earning");

            migrationBuilder.RenameColumn(
                name: "EarntypeId",
                table: "Earning",
                newName: "EarningTypeId");

            migrationBuilder.RenameIndex(
                name: "IX_Earning_EarntypeId",
                table: "Earning",
                newName: "IX_Earning_EarningTypeId");

            migrationBuilder.AddForeignKey(
                name: "FK_Earning_EarningType_EarningTypeId",
                table: "Earning",
                column: "EarningTypeId",
                principalTable: "EarningType",
                principalColumn: "EarntypeId",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Earning_EarningType_EarningTypeId",
                table: "Earning");

            migrationBuilder.RenameColumn(
                name: "EarningTypeId",
                table: "Earning",
                newName: "EarntypeId");

            migrationBuilder.RenameIndex(
                name: "IX_Earning_EarningTypeId",
                table: "Earning",
                newName: "IX_Earning_EarntypeId");

            migrationBuilder.AddForeignKey(
                name: "FK_Earning_EarningType_EarntypeId",
                table: "Earning",
                column: "EarntypeId",
                principalTable: "EarningType",
                principalColumn: "EarntypeId",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
