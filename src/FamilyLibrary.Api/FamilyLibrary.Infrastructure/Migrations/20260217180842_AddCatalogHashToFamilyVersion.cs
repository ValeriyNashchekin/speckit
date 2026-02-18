using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FamilyLibrary.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddCatalogHashToFamilyVersion : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Categories",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    SortOrder = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Categories", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "FamilyNameMappings",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    FamilyName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    RoleName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    ProjectId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    LastSeenAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FamilyNameMappings", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Tags",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Color = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Tags", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "FamilyRoles",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Type = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    CategoryId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FamilyRoles", x => x.Id);
                    table.ForeignKey(
                        name: "FK_FamilyRoles_Categories_CategoryId",
                        column: x => x.CategoryId,
                        principalTable: "Categories",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "Drafts",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    FamilyName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    FamilyUniqueId = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    SelectedRoleId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    TemplateId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    Status = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    LastSeen = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Drafts", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Drafts_FamilyRoles_SelectedRoleId",
                        column: x => x.SelectedRoleId,
                        principalTable: "FamilyRoles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "Families",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    RoleId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    FamilyName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    CurrentVersion = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Families", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Families_FamilyRoles_RoleId",
                        column: x => x.RoleId,
                        principalTable: "FamilyRoles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "FamilyRoleTags",
                columns: table => new
                {
                    FamilyRoleId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TagId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FamilyRoleTags", x => new { x.FamilyRoleId, x.TagId });
                    table.ForeignKey(
                        name: "FK_FamilyRoleTags_FamilyRoles_FamilyRoleId",
                        column: x => x.FamilyRoleId,
                        principalTable: "FamilyRoles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_FamilyRoleTags_Tags_TagId",
                        column: x => x.TagId,
                        principalTable: "Tags",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "RecognitionRules",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    RoleId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    RootNode = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Formula = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RecognitionRules", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RecognitionRules_FamilyRoles_RoleId",
                        column: x => x.RoleId,
                        principalTable: "FamilyRoles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "SystemTypes",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    RoleId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TypeName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Category = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    SystemFamily = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Group = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Json = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CurrentVersion = table.Column<int>(type: "int", nullable: false),
                    Hash = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SystemTypes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SystemTypes_FamilyRoles_RoleId",
                        column: x => x.RoleId,
                        principalTable: "FamilyRoles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "FamilyVersions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    FamilyId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Version = table.Column<int>(type: "int", nullable: false),
                    Hash = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                    PreviousHash = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: true),
                    BlobPath = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    CatalogBlobPath = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    CatalogHash = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: true),
                    OriginalFileName = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    OriginalCatalogName = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    CommitMessage = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    SnapshotJson = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    PublishedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    PublishedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FamilyVersions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_FamilyVersions_Families_FamilyId",
                        column: x => x.FamilyId,
                        principalTable: "Families",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Categories_Name",
                table: "Categories",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Drafts_FamilyUniqueId",
                table: "Drafts",
                column: "FamilyUniqueId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Drafts_SelectedRoleId",
                table: "Drafts",
                column: "SelectedRoleId");

            migrationBuilder.CreateIndex(
                name: "IX_Drafts_Status",
                table: "Drafts",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_Families_RoleId_FamilyName",
                table: "Families",
                columns: new[] { "RoleId", "FamilyName" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_FamilyNameMappings_FamilyName_ProjectId",
                table: "FamilyNameMappings",
                columns: new[] { "FamilyName", "ProjectId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_FamilyNameMappings_RoleName",
                table: "FamilyNameMappings",
                column: "RoleName");

            migrationBuilder.CreateIndex(
                name: "IX_FamilyRoles_CategoryId",
                table: "FamilyRoles",
                column: "CategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_FamilyRoles_Name",
                table: "FamilyRoles",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_FamilyRoleTags_TagId",
                table: "FamilyRoleTags",
                column: "TagId");

            migrationBuilder.CreateIndex(
                name: "IX_FamilyVersions_FamilyId_Version",
                table: "FamilyVersions",
                columns: new[] { "FamilyId", "Version" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_FamilyVersions_Hash",
                table: "FamilyVersions",
                column: "Hash");

            migrationBuilder.CreateIndex(
                name: "IX_RecognitionRules_RoleId",
                table: "RecognitionRules",
                column: "RoleId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_SystemTypes_Hash",
                table: "SystemTypes",
                column: "Hash");

            migrationBuilder.CreateIndex(
                name: "IX_SystemTypes_RoleId_TypeName",
                table: "SystemTypes",
                columns: new[] { "RoleId", "TypeName" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Tags_Name",
                table: "Tags",
                column: "Name",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Drafts");

            migrationBuilder.DropTable(
                name: "FamilyNameMappings");

            migrationBuilder.DropTable(
                name: "FamilyRoleTags");

            migrationBuilder.DropTable(
                name: "FamilyVersions");

            migrationBuilder.DropTable(
                name: "RecognitionRules");

            migrationBuilder.DropTable(
                name: "SystemTypes");

            migrationBuilder.DropTable(
                name: "Tags");

            migrationBuilder.DropTable(
                name: "Families");

            migrationBuilder.DropTable(
                name: "FamilyRoles");

            migrationBuilder.DropTable(
                name: "Categories");
        }
    }
}
