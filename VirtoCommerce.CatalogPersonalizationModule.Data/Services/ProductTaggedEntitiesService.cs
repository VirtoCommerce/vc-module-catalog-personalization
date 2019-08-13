using VirtoCommerce.CatalogPersonalizationModule.Core.Services;
using VirtoCommerce.Domain.Catalog.Model;
using VirtoCommerce.Domain.Catalog.Services;
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
        public IEntity[] GetEntitiesByIds(string[] ids)
        {
            return _itemService.GetByIds(ids, ItemResponseGroup.Outlines);
        }
    }
}
