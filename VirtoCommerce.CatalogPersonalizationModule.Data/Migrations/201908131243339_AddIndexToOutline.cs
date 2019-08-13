namespace VirtoCommerce.CatalogPersonalizationModule.Data.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddIndexToOutline : DbMigration
    {
        public override void Up()
        {
            CreateIndex("dbo.TaggedItemOutline", "Outline");
        }
        
        public override void Down()
        {
            DropIndex("dbo.TaggedItemOutline", new[] { "Outline" });
        }
    }
}
