using System.Collections.Generic;
using System.Linq;
using VirtoCommerce.Domain.Search;

namespace VirtoCommerce.CatalogPersonalizationModule.Data.Search
{
    public static class FiltersHelper
    {
        public static IFilter CreateTermFilter(string fieldName, IEnumerable<string> values)
        {
            return new TermFilter
            {
                FieldName = fieldName,
                Values = values.ToArray(),
            };
        }
    }
}
