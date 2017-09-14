using VirtoCommerce.CatalogPersonalizationModule.Core.Model;
using VirtoCommerce.CatalogPersonalizationModule.Core.Model.Search;
using VirtoCommerce.Domain.Commerce.Model.Search;

namespace VirtoCommerce.CatalogPersonalizationModule.Core.Services
{
    public interface ITaggedItemSearchService
    {
        GenericSearchResult<TaggedItem> SearchTaggedItems(TaggedItemSearchCriteria criteria);
    }
}
