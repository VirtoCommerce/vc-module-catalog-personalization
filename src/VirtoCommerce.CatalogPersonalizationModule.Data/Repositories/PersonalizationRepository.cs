using System;
using System.Linq;
using System.Threading.Tasks;
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
            var items = await GetTaggedItemsByIdsAsync(ids, "None");

            foreach (var item in items)
            {
                Remove(item);
            }
        }

        public Task<TaggedItemOutlineEntity[]> GetTaggedItemOutlinesInsideOutlinesAsync(string[] outlines)
        {
            return TaggedItemOutlines.Where(x => outlines.Any(o => x.Outline.StartsWith(o))).ToArrayAsync();
        }
    }
}
