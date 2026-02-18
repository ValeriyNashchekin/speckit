using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FamilyLibrary.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddCoveringIndexForBatchCheck : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Families_RoleId",
                table: "Families");

            migrationBuilder.CreateIndex(
                name: "IX_Families_RoleId",
                table: "Families",
                column: "RoleId")
                .Annotation("SqlServer:Include", new[] { "CurrentVersion" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Families_RoleId",
                table: "Families");

            migrationBuilder.CreateIndex(
                name: "IX_Families_RoleId",
                table: "Families",
                column: "RoleId");
        }
    }
}
