using System.Collections.Generic;
using System.Linq;
using VirtoCommerce.CatalogModule.Core.Model.Search;
using VirtoCommerce.CatalogModule.Data.Search.Indexing;
using VirtoCommerce.CatalogPersonalizationModule.Data.Common;
using VirtoCommerce.SearchModule.Core.Model;
using VirtoCommerce.SearchModule.Core.Services;

namespace VirtoCommerce.CatalogPersonalizationModule.Data.Search
{
    public class CategorySearchUserGroupsRequestBuilder : CategorySearchRequestBuilder
    {
        public CategorySearchUserGroupsRequestBuilder(ISearchPhraseParser searchPhraseParser)
            : base(searchPhraseParser)
        {
        }

        protected override IList<IFilter> GetFilters(CategoryIndexedSearchCriteria criteria)
        {
            var result = base.GetFilters(criteria);

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
