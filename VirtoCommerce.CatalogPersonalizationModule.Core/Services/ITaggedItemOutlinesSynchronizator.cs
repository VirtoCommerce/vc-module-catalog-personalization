using VirtoCommerce.CatalogPersonalizationModule.Core.Model;

namespace VirtoCommerce.CatalogPersonalizationModule.Core.Services
{
    /// <summary>
    /// Used to synchronize the outlines of tagged entities  with saved data in this module. Used only for UpTree tags propagation policy.
    /// </summary>
    public interface ITaggedItemOutlinesSynchronizator
    {
        void SynchronizeOutlines(TaggedItem[] taggedItems);
    }
}
