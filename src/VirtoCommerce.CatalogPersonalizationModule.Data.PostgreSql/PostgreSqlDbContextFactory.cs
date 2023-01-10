using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using VirtoCommerce.CatalogPersonalizationModule.Data.Repositories;

namespace VirtoCommerce.CatalogPersonalizationModule.Data.PostgreSql
{
    public class PostgreSqlDbContextFactory : IDesignTimeDbContextFactory<PersonalizationDbContext>
    {
        public PersonalizationDbContext CreateDbContext(string[] args)
        {
            var builder = new DbContextOptionsBuilder<PersonalizationDbContext>();
            var connectionString = args.Any() ? args[0] : "User ID = postgres; Password = password; Host = localhost; Port = 5432; Database = virtocommerce3;";

            builder.UseNpgsql(
                connectionString,
                db => db.MigrationsAssembly(typeof(PostgreSqlDbContextFactory).Assembly.GetName().Name));

            return new PersonalizationDbContext(builder.Options);
        }
    }
}
