using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using VirtoCommerce.CatalogPersonalizationModule.Data.Model;
using VirtoCommerce.Platform.Core.Common;

namespace VirtoCommerce.CatalogPersonalizationModule.Data.Repositories
{
    public interface IPersonalizationRepository : IRepository
    {
        DbSet<TaggedItemEntity> TaggedItems { get; }
        DbSet<TagEntity> Tags { get; }
        DbSet<TaggedItemOutlineEntity> TaggedItemOutlines { get; }

        Task<TaggedItemEntity[]> GetTaggedItemsByIdsAsync(string[] ids, string responseGroup);
        Task DeleteTaggedItemsAsync(string[] ids);
    }
}
