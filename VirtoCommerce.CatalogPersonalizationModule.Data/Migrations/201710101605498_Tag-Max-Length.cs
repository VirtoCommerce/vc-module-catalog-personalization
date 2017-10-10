namespace VirtoCommerce.CatalogPersonalizationModule.Data.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class TagMaxLength : DbMigration
    {
        public override void Up()
        {
            AlterColumn("dbo.Tag", "Tag", c => c.String(nullable: false, maxLength: 128));
        }
        
        public override void Down()
        {
            AlterColumn("dbo.Tag", "Tag", c => c.String(nullable: false));
        }
    }
}
