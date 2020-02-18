using System.Threading.Tasks;
using VirtoCommerce.CatalogPersonalizationModule.Core.Model;

namespace VirtoCommerce.CatalogPersonalizationModule.Core.Services
{
    /// <summary>
    /// Provides the APIs for managing tagged items in a persistence store.
    /// </summary>
    public interface ITaggedItemService
    {
        Task<TaggedItem[]> GetByIdsAsync(string[] ids, string responseGroup = null);
        Task SaveChangesAsync(TaggedItem[] taggedItems);
        Task DeleteAsync(string[] ids);
    }
}
