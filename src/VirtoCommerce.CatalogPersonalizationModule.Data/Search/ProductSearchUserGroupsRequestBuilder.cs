using System.Collections.Generic;
using System.Linq;
using VirtoCommerce.CatalogModule.Core.Model.Search;
using VirtoCommerce.CatalogModule.Core.Search;
using VirtoCommerce.CatalogModule.Data.Search.Indexing;
using VirtoCommerce.CatalogPersonalizationModule.Data.Common;
using VirtoCommerce.SearchModule.Core.Model;
using VirtoCommerce.SearchModule.Core.Services;

namespace VirtoCommerce.CatalogPersonalizationModule.Data.Search
{
    public class ProductSearchUserGroupsRequestBuilder : ProductSearchRequestBuilder
    {
        public ProductSearchUserGroupsRequestBuilder(ISearchPhraseParser searchPhraseParser, ITermFilterBuilder termFilterBuilder,
            IAggregationConverter aggregationConverter)
            : base(searchPhraseParser, termFilterBuilder, aggregationConverter)
        {
        }

        protected override IList<IFilter> GetPermanentFilters(ProductIndexedSearchCriteria criteria)
        {
            var result = base.GetPermanentFilters(criteria);

            if (criteria.UserGroups != null)
            {
                var userGroups = criteria.UserGroups.ToList();

                if (!userGroups.Any())
                {
                    userGroups.Add(Constants.UserGroupsAnyValue);
                }

                result.Add(new TermFilter { FieldName = Constants.UserGroupsFieldName, Values = userGroups });
            }

            return result;
        }
    }
}
