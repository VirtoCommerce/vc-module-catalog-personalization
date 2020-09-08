using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using VirtoCommerce.CatalogPersonalizationModule.Core.Model.Search;
using VirtoCommerce.CatalogPersonalizationModule.Data.Model;
using VirtoCommerce.Platform.Core.Common;
using VirtoCommerce.Platform.Data.Infrastructure;


namespace VirtoCommerce.CatalogPersonalizationModule.Data.Repositories
{
    public class PersonalizationRepository : DbContextRepositoryBase<PersonalizationDbContext>, IPersonalizationRepository
    {
        public PersonalizationRepository(PersonalizationDbContext dbContext) : base(dbContext)
        {
        }

        public IQueryable<TaggedItemEntity> TaggedItems => DbContext.Set<TaggedItemEntity>().Include(x => x.Tags);

        public IQueryable<TagEntity> Tags => DbContext.Set<TagEntity>();

        public IQueryable<TaggedItemOutlineEntity> TaggedItemOutlines => DbContext.Set<TaggedItemOutlineEntity>();

        public async Task<TaggedItemEntity[]> GetTaggedItemsByIdsAsync(string[] ids, string responseGroup)
        {
            var result = Array.Empty<TaggedItemEntity>();
            if (!ids.IsNullOrEmpty())
            {
                var taggedItemsGroup = EnumUtility.SafeParse(responseGroup, TaggedItemResponseGroup.Full);

                result = await TaggedItems.Where(x => ids.Contains(x.Id)).ToArrayAsync();

                if (taggedItemsGroup.HasFlag(TaggedItemResponseGroup.WithOutlines))
                {
#pragma warning disable S1481 // Unused local variables should be removed
                    // Variable needed to avoid possible optimization as result is never used
                    var outlines = await TaggedItemOutlines.Where(x => ids.Contains(x.TaggedItemId)).ToArrayAsync();
#pragma warning restore S1481 // Unused local variables should be removed
                }
            }
            return result;
        }

        public async Task DeleteTaggedItemsAsync(string[] ids)
        {
            await ExecuteStoreCommand("DELETE FROM TaggedItem WHERE Id IN ({0})", ids);
        }

        protected virtual async Task ExecuteStoreCommand(string commandTemplate, IEnumerable<string> parameterValues)
        {
            var command = CreateCommand(commandTemplate, parameterValues);
            await DbContext.Database.ExecuteSqlRawAsync(command.Text, command.Parameters);
        }

        protected virtual Command CreateCommand(string commandTemplate, IEnumerable<string> parameterValues)
        {
            var parameters = parameterValues.Select((v, i) => new SqlParameter($"@p{i}", v)).ToArray();
            var parameterNames = string.Join(",", parameters.Select(p => p.ParameterName));

            return new Command
            {
                Text = string.Format(commandTemplate, parameterNames),
                Parameters = parameters.OfType<object>().ToArray(),
            };
        }


        public async Task DeleteTaggedItemOutlines(string[] ids)
        {
            await ExecuteStoreCommand("DELETE FROM [TaggedItemOutline] WHERE Id IN ({0})", ids);
        }

        public Task<TaggedItemOutlineEntity[]> GetTaggedItemOutlinesInsideOutlinesAsync(string[] outlines)
        {
            var outlinesString = string.Join(',', outlines);

            var result = DbContext.Set<TaggedItemOutlineEntity>()
                // Line below is ".Where(x => outlines.Any(o => x.Outline.StartsWith(o)))" properly translated into SQL.
                // It could not be translated to SQL query by EF Core 3.1.
                .FromSqlInterpolated($"SELECT * FROM [TaggedItemOutline] t JOIN STRING_SPLIT({outlinesString}, ',') outline ON t.Outline LIKE outline.value + '%'")
                .ToArrayAsync();

            return result;
        }

        protected class Command
        {
            public string Text { get; set; }
            public object[] Parameters { get; set; }
        }
    }
}
