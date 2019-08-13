using VirtoCommerce.CatalogPersonalizationModule.Core.Services;
using VirtoCommerce.Domain.Catalog.Model;
using VirtoCommerce.Domain.Catalog.Services;
using VirtoCommerce.Platform.Core.Common;

namespace VirtoCommerce.CatalogPersonalizationModule.Data.Services
{
    public class CategoryTaggedEntitiesService : ITaggedEntitiesService
    {
        private readonly ICategoryService _categoryService;

        public CategoryTaggedEntitiesService(ICategoryService categoryService)
        {
            _categoryService = categoryService;
        }

        public IEntity[] GetEntitiesByIds(string[] ids)
        {
            return _categoryService.GetByIds(ids, CategoryResponseGroup.WithOutlines);
        }
    }
}
