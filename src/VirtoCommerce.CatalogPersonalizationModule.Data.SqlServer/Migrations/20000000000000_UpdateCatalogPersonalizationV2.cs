using Microsoft.EntityFrameworkCore.Migrations;

namespace VirtoCommerce.CatalogPersonalizationModule.Data.SqlServer.Migrations
{
    public partial class UpdateCatalogPersonalizationV2 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"IF (EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = '__MigrationHistory'))
        IF (EXISTS (SELECT * FROM __MigrationHistory WHERE ContextKey = 'VirtoCommerce.CatalogPersonalizationModule.Data.Migrations.Configuration'))
            BEGIN
                INSERT INTO [__EFMigrationsHistory] ([MigrationId],[ProductVersion]) VALUES ('20200217121812_InitialCatalogPersonalization', '2.2.3-servicing-35854')
            END");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
        }
    }
}
