using System;
using VirtoCommerce.Domain.Commerce.Model.Search;

namespace VirtoCommerce.CatalogPersonalizationModule.Core.Model.Search
{
    public class TaggedItemSearchCriteria : SearchCriteriaBase
    {
        public string EntityId { get; set; }
        public DateTime? ChangedFrom { get; set; }
        public string EntityType { get; set; }
        public string[] Ids { get; set; }
    }
}
