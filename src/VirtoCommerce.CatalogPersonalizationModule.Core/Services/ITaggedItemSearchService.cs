using System.Threading.Tasks;
using VirtoCommerce.CatalogPersonalizationModule.Core.Model;
using VirtoCommerce.CatalogPersonalizationModule.Core.Model.Search;
using VirtoCommerce.Platform.Core.Common;

namespace VirtoCommerce.CatalogPersonalizationModule.Core.Services
{
    /// <summary>
    /// Provides an abstraction for search  tagged items 
    /// </summary>
    public interface ITaggedItemSearchService
    {
        Task<GenericSearchResult<TaggedItem>> SearchTaggedItemsAsync(TaggedItemSearchCriteria criteria);
    }
}
