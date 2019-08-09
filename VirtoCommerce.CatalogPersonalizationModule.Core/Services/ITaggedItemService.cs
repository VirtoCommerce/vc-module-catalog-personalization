using VirtoCommerce.CatalogPersonalizationModule.Core.Model;

namespace VirtoCommerce.CatalogPersonalizationModule.Core.Services
{
	public interface ITaggedItemService
	{
		TaggedItem[] GetTaggedItemsByIds(string[] ids);
		TaggedItem[] GetTaggedItemsByIds(string[] ids, string responseGroup);

		TaggedItem[] GetTaggedItemsByObjectIds(string[] ids);

		void SaveTaggedItems(TaggedItem[] taggedItems);

		void DeleteTaggedItems(string[] ids);
	}
}
