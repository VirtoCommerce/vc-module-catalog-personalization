namespace VirtoCommerce.CatalogPersonalizationModule.Data.Migrations
{
    using System.Data.Entity.Migrations;

    public sealed class Configuration : DbMigrationsConfiguration<Repositories.PersonalizationRepositoryImpl>
    {
        public Configuration()
        {
            AutomaticMigrationsEnabled = false;
        }

        protected override void Seed(Repositories.PersonalizationRepositoryImpl context)
        {
            // Do nothing because we needn't seed anything yet.
        }
    }
}
