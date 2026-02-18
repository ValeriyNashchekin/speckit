using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FamilyLibrary.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddPhase3Entities : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "FamilyDependencies",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ParentFamilyId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    NestedFamilyName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    NestedRoleName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    IsShared = table.Column<bool>(type: "bit", nullable: false),
                    InLibrary = table.Column<bool>(type: "bit", nullable: false),
                    LibraryVersion = table.Column<int>(type: "int", nullable: true),
                    DetectedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FamilyDependencies", x => x.Id);
                    table.ForeignKey(
                        name: "FK_FamilyDependencies_Families_ParentFamilyId",
                        column: x => x.ParentFamilyId,
                        principalTable: "Families",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "MaterialMappings",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ProjectId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TemplateMaterialName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    ProjectMaterialName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    LastUsedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MaterialMappings", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_FamilyDependency_NestedRoleName",
                table: "FamilyDependencies",
                column: "NestedRoleName",
                filter: "[NestedRoleName] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_FamilyDependency_ParentFamilyId",
                table: "FamilyDependencies",
                column: "ParentFamilyId");

            migrationBuilder.CreateIndex(
                name: "IX_MaterialMapping_ProjectId",
                table: "MaterialMappings",
                column: "ProjectId");

            migrationBuilder.CreateIndex(
                name: "IX_MaterialMapping_ProjectId_TemplateName",
                table: "MaterialMappings",
                columns: new[] { "ProjectId", "TemplateMaterialName" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "FamilyDependencies");

            migrationBuilder.DropTable(
                name: "MaterialMappings");
        }
    }
}
