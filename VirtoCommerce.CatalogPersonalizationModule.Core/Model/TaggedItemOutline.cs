using VirtoCommerce.Platform.Core.Common;

namespace VirtoCommerce.CatalogPersonalizationModule.Core.Model
{
	public class TaggedItemOutline : Entity
	{
		public string Outline { get; set; }

        public TaggedItem TaggedItem { get; set; }
        public string TaggedItemId { get; set; }
	}
}
