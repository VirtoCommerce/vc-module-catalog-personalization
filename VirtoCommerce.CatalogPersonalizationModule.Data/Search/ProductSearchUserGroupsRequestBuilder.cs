using System.Collections.Generic;
using System.Linq;
using VirtoCommerce.CatalogModule.Data.Search;
using VirtoCommerce.Domain.Catalog.Model.Search;
using VirtoCommerce.Domain.Search;

namespace VirtoCommerce.CatalogPersonalizationModule.Data.Search
{
    public class ProductSearchUserGroupsRequestBuilder : ProductSearchRequestBuilder, ISearchRequestBuilder
    {
        public ProductSearchUserGroupsRequestBuilder(ISearchPhraseParser searchPhraseParser, ITermFilterBuilder termFilterBuilder, IAggregationConverter aggregationConverter) : 
            base(searchPhraseParser, termFilterBuilder, aggregationConverter)
        {
        }

        protected override IList<IFilter> GetPermanentFilters(ProductSearchCriteria criteria)
        {
            List<IFilter> result = base.GetPermanentFilters(criteria).ToList();

            var userGroups = criteria.GetUserGroups();
            result.AddRange(userGroups.Select(term => FiltersHelper.CreateTermFilter(term.Key, term.Values)));

            return result;
        }
    }
}
