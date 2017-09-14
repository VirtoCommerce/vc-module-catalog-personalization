namespace VirtoCommerce.CatalogPersonalizationModule.Data.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Initial : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.Tag",
                c => new
                    {
                        Id = c.String(nullable: false, maxLength: 128),
                        Tag = c.String(nullable: false),
                        TaggedItemId = c.String(nullable: false, maxLength: 128),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.TaggedItem", t => t.TaggedItemId, cascadeDelete: true)
                .Index(t => t.TaggedItemId);
            
            CreateTable(
                "dbo.TaggedItem",
                c => new
                    {
                        Id = c.String(nullable: false, maxLength: 128),
                        Label = c.String(nullable: false, maxLength: 128),
                        ObjectType = c.String(nullable: false, maxLength: 128),
                        ObjectId = c.String(nullable: false, maxLength: 128),
                        CreatedDate = c.DateTime(nullable: false),
                        ModifiedDate = c.DateTime(),
                        CreatedBy = c.String(maxLength: 64),
                        ModifiedBy = c.String(maxLength: 64),
                    })
                .PrimaryKey(t => t.Id);
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.Tag", "TaggedItemId", "dbo.TaggedItem");
            DropIndex("dbo.Tag", new[] { "TaggedItemId" });
            DropTable("dbo.TaggedItem");
            DropTable("dbo.Tag");
        }
    }
}
