using System;
using System.Collections.Generic;
using System.Linq;
using VirtoCommerce.CatalogPersonalizationModule.Data.Common;
using VirtoCommerce.Domain.Catalog.Model.Search;

namespace VirtoCommerce.CatalogPersonalizationModule.Data.Search
{
    public static class SearchCriteriaExtensions
    {
        public static IList<StringKeyValues> GetUserGroups(this CatalogSearchCriteriaBase criteria)
        {
            var result = new List<StringKeyValues>();

            if (criteria.UserGroups != null && criteria.UserGroups.Any())
            {
                var userGroupValues = criteria.UserGroups.ToList();
                userGroupValues.Add(Constants.UserGroupDefaultValue);

                StringKeyValues userGroupCriteria = new StringKeyValues { Key = Constants.UserGroupKey, Values = userGroupValues.ToArray() };
                result.Add(userGroupCriteria);
            }

            return result;
        }
    }

    public class StringKeyValues
    {
        public string Key { get; set; }
        public string[] Values { get; set; }
    }
}
