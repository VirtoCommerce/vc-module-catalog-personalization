namespace VirtoCommerce.CatalogPersonalizationModule.Data.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddTaggedItemOutlines : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.TaggedItemOutline",
                c => new
                    {
                        Id = c.String(nullable: false, maxLength: 128),
                        Outline = c.String(nullable: false, maxLength: 2048),
                        TaggedItemId = c.String(nullable: false, maxLength: 128),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.TaggedItem", t => t.TaggedItemId, cascadeDelete: true)
                .Index(t => t.TaggedItemId);
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.TaggedItemOutline", "TaggedItemId", "dbo.TaggedItem");
            DropIndex("dbo.TaggedItemOutline", new[] { "TaggedItemId" });
            DropTable("dbo.TaggedItemOutline");
        }
    }
}
