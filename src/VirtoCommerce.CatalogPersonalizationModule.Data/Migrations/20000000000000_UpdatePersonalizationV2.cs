using Microsoft.EntityFrameworkCore.Migrations;

namespace VirtoCommerce.CatalogPersonalizationModule.Data.Migrations
{
    public partial class UpdateCustomerReviewsV2 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"IF (EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = '__MigrationHistory'))
        IF (EXISTS (SELECT * FROM __MigrationHistory WHERE ContextKey = 'VirtoCommerce.CatalogPersonalization.Data.Migrations.Configuration'))
            BEGIN
                INSERT INTO [dbo].[__EFMigrationsHistory] ([MigrationId],[ProductVersion]) VALUES ('20200212123205_InitialPersonalization', '2.2.3-servicing-35854')
            END");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {

        }
    }
}
