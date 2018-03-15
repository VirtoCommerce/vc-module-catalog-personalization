using System.Collections.Generic;
using System.Linq;
using VirtoCommerce.CatalogModule.Data.Search;
using VirtoCommerce.CatalogPersonalizationModule.Data.Common;
using VirtoCommerce.Domain.Catalog.Model.Search;
using VirtoCommerce.Domain.Search;

namespace VirtoCommerce.CatalogPersonalizationModule.Data.Search
{
    public class ProductSearchUserGroupsRequestBuilder : ProductSearchRequestBuilder
    {
        public ProductSearchUserGroupsRequestBuilder(ISearchPhraseParser searchPhraseParser, ITermFilterBuilder termFilterBuilder, IAggregationConverter aggregationConverter)
            : base(searchPhraseParser, termFilterBuilder, aggregationConverter)
        {
        }

        protected override IList<IFilter> GetPermanentFilters(ProductSearchCriteria criteria)
        {
            var result = base.GetPermanentFilters(criteria);

            var userGroups = criteria.UserGroups?.ToList() ?? new List<string>();
            userGroups.Add(Constants.UserGroupsAnyValue);

            result.Add(new TermFilter { FieldName = Constants.UserGroupsFieldName, Values = userGroups });

            return result;
        }
    }
}
