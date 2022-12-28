using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace VirtoCommerce.CatalogPersonalizationModule.Data.SqlServer.Migrations
{
    public partial class RenameIndex : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameIndex(
                name: "IX_Outline",
                table: "TaggedItemOutline",
                newName: "IX_TaggedItemOutlineEntity_Outline");

            migrationBuilder.RenameIndex(
                name: "IX_ObjectId_ObjectType",
                table: "TaggedItem",
                newName: "IX_TaggedItem_ObjectId_ObjectType");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameIndex(
                name: "IX_TaggedItemOutlineEntity_Outline",
                table: "TaggedItemOutline",
                newName: "IX_Outline");

            migrationBuilder.RenameIndex(
                name: "IX_TaggedItem_ObjectId_ObjectType",
                table: "TaggedItem",
                newName: "IX_ObjectId_ObjectType");
        }
    }
}
