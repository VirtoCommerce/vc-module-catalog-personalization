using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace VirtoCommerce.CatalogPersonalizationModule.Data.PostgreSql.Migrations
{
    public partial class Initial : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "TaggedItem",
                columns: table => new
                {
                    Id = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    Label = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    ObjectType = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    ObjectId = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ModifiedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedBy = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    ModifiedBy = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TaggedItem", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Tag",
                columns: table => new
                {
                    Id = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    Tag = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    TaggedItemId = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false)
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
                    Id = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    Outline = table.Column<string>(type: "character varying(2048)", maxLength: 2048, nullable: false),
                    TaggedItemId = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false)
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
                name: "IX_TaggedItem_ObjectId_ObjectType",
                table: "TaggedItem",
                columns: new[] { "ObjectId", "ObjectType" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_TaggedItemOutline_TaggedItemId",
                table: "TaggedItemOutline",
                column: "TaggedItemId");

            migrationBuilder.CreateIndex(
                name: "IX_TaggedItemOutlineEntity_Outline",
                table: "TaggedItemOutline",
                column: "Outline");
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
