using System.Linq;
using System.Threading.Tasks;
using VirtoCommerce.CatalogPersonalizationModule.Data.Model;
using VirtoCommerce.Platform.Core.Common;

namespace VirtoCommerce.CatalogPersonalizationModule.Data.Repositories
{
    public interface IPersonalizationRepository : IRepository
    {
        IQueryable<TaggedItemEntity> TaggedItems { get; }
        IQueryable<TagEntity> Tags { get; }
        IQueryable<TaggedItemOutlineEntity> TaggedItemOutlines { get; }

        Task<TaggedItemEntity[]> GetTaggedItemsByIdsAsync(string[] ids, string responseGroup);
        Task DeleteTaggedItemsAsync(string[] ids);
    }
}
