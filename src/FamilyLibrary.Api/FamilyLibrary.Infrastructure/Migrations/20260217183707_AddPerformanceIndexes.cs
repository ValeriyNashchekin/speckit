using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FamilyLibrary.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddPerformanceIndexes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_SystemTypes_Category_TypeName",
                table: "SystemTypes",
                columns: new[] { "Category", "TypeName" });

            migrationBuilder.CreateIndex(
                name: "IX_SystemTypes_RoleId",
                table: "SystemTypes",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "IX_FamilyVersions_FamilyId",
                table: "FamilyVersions",
                column: "FamilyId");

            migrationBuilder.CreateIndex(
                name: "IX_FamilyRoles_Type",
                table: "FamilyRoles",
                column: "Type");

            migrationBuilder.CreateIndex(
                name: "IX_Families_FamilyName",
                table: "Families",
                column: "FamilyName");

            migrationBuilder.CreateIndex(
                name: "IX_Families_RoleId",
                table: "Families",
                column: "RoleId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_SystemTypes_Category_TypeName",
                table: "SystemTypes");

            migrationBuilder.DropIndex(
                name: "IX_SystemTypes_RoleId",
                table: "SystemTypes");

            migrationBuilder.DropIndex(
                name: "IX_FamilyVersions_FamilyId",
                table: "FamilyVersions");

            migrationBuilder.DropIndex(
                name: "IX_FamilyRoles_Type",
                table: "FamilyRoles");

            migrationBuilder.DropIndex(
                name: "IX_Families_FamilyName",
                table: "Families");

            migrationBuilder.DropIndex(
                name: "IX_Families_RoleId",
                table: "Families");
        }
    }
}
