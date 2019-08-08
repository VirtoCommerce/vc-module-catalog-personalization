using System.Collections.Generic;
using System.Linq;
using VirtoCommerce.CatalogModule.Data.Search;
using VirtoCommerce.CatalogPersonalizationModule.Data.Common;
using VirtoCommerce.Domain.Catalog.Model.Search;
using VirtoCommerce.Domain.Search;

namespace VirtoCommerce.CatalogPersonalizationModule.Data.Search
{
	public class CategorySearchUserGroupsRequestBuilder : CategorySearchRequestBuilder
	{
		public CategorySearchUserGroupsRequestBuilder(ISearchPhraseParser searchPhraseParser)
			: base(searchPhraseParser)
		{
		}

		protected override IList<IFilter> GetFilters(CategorySearchCriteria criteria)
		{
			var result = base.GetFilters(criteria);

			if (criteria.UserGroups != null)
			{
				var userGroups = criteria.UserGroups.ToList();
				userGroups.Add(Constants.UserGroupsAnyValue);

				result.Add(new TermFilter { FieldName = Constants.UserGroupsFieldName, Values = userGroups });
			}

			return result;
		}
	}
}
