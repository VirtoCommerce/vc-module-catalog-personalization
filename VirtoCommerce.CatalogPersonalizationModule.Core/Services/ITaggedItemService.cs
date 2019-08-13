using VirtoCommerce.CatalogPersonalizationModule.Core.Model;

namespace VirtoCommerce.CatalogPersonalizationModule.Core.Services
{
    /// <summary>
    /// Provides the APIs for managing tagged items in a persistence store.
    /// </summary>
	public interface ITaggedItemService
	{
		TaggedItem[] GetTaggedItemsByIds(string[] ids, string responseGroup = null);
        void SaveTaggedItems(TaggedItem[] taggedItems);
		void DeleteTaggedItems(string[] ids);
	}
}
