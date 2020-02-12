using System.Threading.Tasks;
using VirtoCommerce.CatalogPersonalizationModule.Core.Model;

namespace VirtoCommerce.CatalogPersonalizationModule.Core.Services
{
    /// <summary>
    /// Provides the APIs for managing tagged items in a persistence store.
    /// </summary>
    public interface ITaggedItemService
    {
        Task<TaggedItem[]> GetTaggedItemsByIdsAsync(string[] ids, string responseGroup = null);
        Task SaveTaggedItemsAsync(TaggedItem[] taggedItems);
        Task DeleteTaggedItemsAsync(string[] ids);
    }
}
