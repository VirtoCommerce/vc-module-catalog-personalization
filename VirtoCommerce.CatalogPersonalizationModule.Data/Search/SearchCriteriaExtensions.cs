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

            if (criteria.UserGroups != null)
            {
                var nameValueDelimeter = new[] { ':' };
                var valuesDelimeter = new[] { ',' };

                StringKeyValues userGroupInfo = criteria.UserGroups
                    .Select(item => item.Split(nameValueDelimeter, 2))
                    .Where(item => item.Length == 2)
                    .Select(item => new StringKeyValues { Key = item[0], Values = item[1].Split(valuesDelimeter, StringSplitOptions.RemoveEmptyEntries) })
                    .FirstOrDefault();

                if (userGroupInfo != null) {
                    var values = userGroupInfo.Values.ToList();
                    values.Add(Constants.UserGroupDefaultValue);
                    userGroupInfo.Values = values.ToArray();
                    result.Add(userGroupInfo);
                }
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
