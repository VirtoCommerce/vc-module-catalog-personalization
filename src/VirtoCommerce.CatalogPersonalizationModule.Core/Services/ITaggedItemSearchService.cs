using System.Threading.Tasks;
using VirtoCommerce.CatalogPersonalizationModule.Core.Model.Search;

namespace VirtoCommerce.CatalogPersonalizationModule.Core.Services
{
    /// <summary>
    /// Provides an abstraction for search  tagged items 
    /// </summary>
    public interface ITaggedItemSearchService
    {
        Task<TaggedItemSearchResult> SearchTaggedItemsAsync(TaggedItemSearchCriteria criteria);
    }
}
