using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace VirtoCommerce.CatalogPersonalizationModule.Data.Migrations
{
    public partial class InitialCatalogPersonalization : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "TaggedItem",
                columns: table => new
                {
                    Id = table.Column<string>(maxLength: 128, nullable: false),
                    CreatedDate = table.Column<DateTime>(nullable: false),
                    ModifiedDate = table.Column<DateTime>(nullable: true),
                    CreatedBy = table.Column<string>(maxLength: 64, nullable: true),
                    ModifiedBy = table.Column<string>(maxLength: 64, nullable: true),
                    Label = table.Column<string>(maxLength: 128, nullable: false),
                    ObjectType = table.Column<string>(maxLength: 128, nullable: false),
                    ObjectId = table.Column<string>(maxLength: 128, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TaggedItem", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Tag",
                columns: table => new
                {
                    Id = table.Column<string>(maxLength: 128, nullable: false),
                    Tag = table.Column<string>(maxLength: 128, nullable: false),
                    TaggedItemId = table.Column<string>(maxLength: 128, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Tag", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Tag_TaggedItem_TaggedItemId",
                        column: x => x.TaggedItemId,
                        principalTable: "TaggedItem",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "TaggedItemOutline",
                columns: table => new
                {
                    Id = table.Column<string>(maxLength: 128, nullable: false),
                    Outline = table.Column<string>(maxLength: 2048, nullable: false),
                    TaggedItemId = table.Column<string>(maxLength: 128, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TaggedItemOutline", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TaggedItemOutline_TaggedItem_TaggedItemId",
                        column: x => x.TaggedItemId,
                        principalTable: "TaggedItem",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Tag_TaggedItemId",
                table: "Tag",
                column: "TaggedItemId");

            migrationBuilder.CreateIndex(
                name: "IX_Outline",
                table: "TaggedItemOutline",
                column: "Outline");

            migrationBuilder.CreateIndex(
                name: "IX_TaggedItemOutline_TaggedItemId",
                table: "TaggedItemOutline",
                column: "TaggedItemId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Tag");

            migrationBuilder.DropTable(
                name: "TaggedItemOutline");

            migrationBuilder.DropTable(
                name: "TaggedItem");
        }
    }
}
