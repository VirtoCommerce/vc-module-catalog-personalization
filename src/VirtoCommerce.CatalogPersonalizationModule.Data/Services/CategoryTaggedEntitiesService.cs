using System.Linq;
using System.Threading.Tasks;
using VirtoCommerce.CatalogModule.Core.Model;
using VirtoCommerce.CatalogModule.Core.Services;
using VirtoCommerce.CatalogPersonalizationModule.Core.Services;
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

        public async Task<IEntity[]> GetEntitiesByIdsAsync(string[] ids)
        {
            return (await _categoryService.GetAsync(ids, CategoryResponseGroup.WithOutlines.ToString())).ToArray<IEntity>();
        }
    }
}
