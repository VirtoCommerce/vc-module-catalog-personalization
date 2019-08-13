namespace VirtoCommerce.CatalogPersonalizationModule.Core.Services
{
    /// <summary>
    /// Creates an ITaggedEntitiesService instance based on the given type name string.
    /// </summary>
    public interface ITaggedEntitiesServiceFactory
    {
        ITaggedEntitiesService Create(string entityType);
    }
}
