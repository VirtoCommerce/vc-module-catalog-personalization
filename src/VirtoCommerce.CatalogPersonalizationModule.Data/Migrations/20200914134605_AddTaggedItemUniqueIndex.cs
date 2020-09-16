using Microsoft.EntityFrameworkCore.Migrations;

namespace VirtoCommerce.CatalogPersonalizationModule.Data.Migrations
{
    public partial class AddTaggedItemUniqueIndex : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_ObjectId_ObjectType",
                table: "TaggedItem",
                columns: new[] { "ObjectId", "ObjectType" },
                unique: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_ObjectId_ObjectType",
                table: "TaggedItem");
        }
    }
}
