using VirtoCommerce.CatalogPersonalizationModule.Core.Model;

namespace VirtoCommerce.CatalogPersonalizationModule.Core.Services
{
	public interface ITaggedItemOutlineService
	{
		string[] GetTagsByOutlinePart(string outlinePart);

		void SaveTaggedItemOutlines(TaggedItemOutline[] taggedItemOutlines);

		void DeleteTaggedItemOutlines(string[] ids);
	}
}
