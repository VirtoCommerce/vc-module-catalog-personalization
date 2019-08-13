using System.Collections.Generic;
using VirtoCommerce.Platform.Core.Common;

namespace VirtoCommerce.CatalogPersonalizationModule.Core.Services
{
    /// <summary>
    /// Provides an abstraction for evaluate resulting tags for concrete entities with using one of type tags propagation strategy that might
    /// depends from object position in hierarchy 
    /// </summary>
    public interface ITagPropagationPolicy
    {
        Dictionary<string, HashSet<string>> GetResultingTags(IEntity[] entities);
    }
}
