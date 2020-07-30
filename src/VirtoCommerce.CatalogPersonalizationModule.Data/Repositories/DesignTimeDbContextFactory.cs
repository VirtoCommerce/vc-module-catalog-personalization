using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace VirtoCommerce.CatalogPersonalizationModule.Data.Repositories
{
    public class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<PersonalizationDbContext>
    {
        public PersonalizationDbContext CreateDbContext(string[] args)
        {
            var builder = new DbContextOptionsBuilder<PersonalizationDbContext>();

            builder.UseSqlServer("Data Source=(local);Initial Catalog=VirtoCommerce3;Persist Security Info=True;User ID=virto;Password=virto;MultipleActiveResultSets=True;Connect Timeout=30");

            return new PersonalizationDbContext(builder.Options);
        }
    }
}
