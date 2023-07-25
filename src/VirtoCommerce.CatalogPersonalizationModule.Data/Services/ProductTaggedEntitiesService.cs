using System.Linq;
using System.Threading.Tasks;
using VirtoCommerce.CatalogModule.Core.Model;
using VirtoCommerce.CatalogModule.Core.Services;
using VirtoCommerce.CatalogPersonalizationModule.Core.Services;
using VirtoCommerce.Platform.Core.Common;

namespace VirtoCommerce.CatalogPersonalizationModule.Data.Services
{
    public class ProductTaggedEntitiesService : ITaggedEntitiesService
    {
        private readonly IItemService _itemService;

        public ProductTaggedEntitiesService(IItemService itemService)
        {
            _itemService = itemService;
        }

        public async Task<IEntity[]> GetEntitiesByIdsAsync(string[] ids)
        {
            return (await _itemService.GetAsync(ids, ItemResponseGroup.Outlines.ToString())).ToArray<IEntity>();
        }
    }
}
