using VirtoCommerce.CatalogPersonalizationModule.Core.Model;
using VirtoCommerce.CatalogPersonalizationModule.Core.Model.Search;
using VirtoCommerce.Domain.Commerce.Model.Search;

namespace VirtoCommerce.CatalogPersonalizationModule.Core.Services
{
    /// <summary>
    /// Provides an abstraction for search  tagged items 
    /// </summary>
    public interface ITaggedItemSearchService
    {
        GenericSearchResult<TaggedItem> SearchTaggedItems(TaggedItemSearchCriteria criteria);
    }
}
