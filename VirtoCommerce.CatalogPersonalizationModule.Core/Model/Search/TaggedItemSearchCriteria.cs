using System;
using VirtoCommerce.Domain.Commerce.Model.Search;

namespace VirtoCommerce.CatalogPersonalizationModule.Core.Model.Search
{
	public class TaggedItemSearchCriteria : SearchCriteriaBase
	{
		public string EntityId { get; set; }

		private string[] entityIds;
		public string[] EntityIds
		{
			get
			{
				if (entityIds == null && !string.IsNullOrEmpty(EntityId))
				{
					entityIds = new[] { EntityId };
				}
				return entityIds;
			}
			set
			{
				entityIds = value;
			}
		}

		public DateTime? ChangedFrom { get; set; }
		public string EntityType { get; set; }
		public string[] Ids { get; set; }
	}
}
