using System.Threading.Tasks;
using VirtoCommerce.Platform.Core.Common;

namespace VirtoCommerce.CatalogPersonalizationModule.Core.Services
{
    /// <summary>
    /// Provides an abstraction for load tagged entities real objects by their identifiers
    /// </summary>
    public interface ITaggedEntitiesService
    {
        Task<IEntity[]> GetEntitiesByIdsAsync(string[] ids);
    }
}
