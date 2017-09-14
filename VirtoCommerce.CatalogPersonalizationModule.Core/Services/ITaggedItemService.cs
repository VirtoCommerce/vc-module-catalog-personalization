using VirtoCommerce.CatalogPersonalizationModule.Core.Model;

namespace VirtoCommerce.CatalogPersonalizationModule.Core.Services
{
    public interface ITaggedItemService
    {
        TaggedItem[] GetTaggedItemsByIds(string[] ids);

        void SaveTaggedItems(TaggedItem[] taggedItems);

        void DeleteTaggedItems(string[] ids);
    }
}
