using Microsoft.EntityFrameworkCore.Migrations;

namespace VirtoCommerce.CatalogPersonalizationModule.Data.Migrations
{
    public partial class AddTaggedItemUniqueIndex : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
	            -- 1. Set Tag.TaggedItemId to one element in all tags belonging to one ObjectId using different tag items
	            ;WITH TagDuplicates (Id, TaggedItemId, first_tagged_item_id) AS (
		            SELECT 
			            t.Id,
			            t.TaggedItemId,
			            FIRST_VALUE(t.TaggedItemId) OVER(
				            PARTITION BY 
					            [ObjectId], 
					            [ObjectType]
				            ORDER BY 
					            t.TaggedItemId
			            ) as first_tagged_item_id
			            FROM [TaggedItem] ti 
			            INNER JOIN [Tag] t ON ti.Id = t.TaggedItemId
	            )
	            UPDATE t
	            SET 
		            t.TaggedItemId = TagDuplicates.first_tagged_item_id
	            FROM Tag t INNER JOIN TagDuplicates ON t.Id = TagDuplicates.Id		
	            WHERE t.TaggedItemId <> first_tagged_item_id;

	            -- 2. Delete Tag duplicates
	            ;WITH cte AS (
		            SELECT 
			            Tag, 
			            TaggedItemId, 
			            ROW_NUMBER() OVER (
				            PARTITION BY 
					            Tag, 
					            TaggedItemId
				            ORDER BY 
					            [Id]
			            ) row_num
			            FROM 
			            [Tag]
	            )
	            DELETE FROM cte
	            WHERE row_num > 1;

	            -- 3. Delete empty TaggedItem's (former duplicates without Tags now)
	            DELETE FROM [TaggedItem]
	            WHERE NOT EXISTS (SELECT 1 FROM [Tag] Where [TaggedItem].Id = [Tag].TaggedItemId);
            ");

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
