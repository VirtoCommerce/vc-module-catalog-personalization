using System;
using VirtoCommerce.CatalogModule.Core.Services;
using VirtoCommerce.CatalogPersonalizationModule.Core.Services;
using VirtoCommerce.Platform.Core.Common;
using VirtoCommerce.SearchModule.Core.Model;

namespace VirtoCommerce.CatalogPersonalizationModule.Data.Services
{
    public class TaggedEntitiesServiceFactory : ITaggedEntitiesServiceFactory
    {
        private readonly ICategoryService _categoryService;
        private readonly IItemService _itemService;
        public TaggedEntitiesServiceFactory(ICategoryService categoryService, IItemService itemService)
        {
            _categoryService = categoryService;
            _itemService = itemService;
        }

        public ITaggedEntitiesService Create(string entityType)
        {
            if(entityType == null)
            {
                throw new ArgumentNullException(nameof(entityType));
            }

            ITaggedEntitiesService result;
            if (entityType.EqualsInvariant(KnownDocumentTypes.Category))
            {
                result = new CategoryTaggedEntitiesService(_categoryService);
            }
            else if (entityType.EqualsInvariant(KnownDocumentTypes.Product))
            {
                result = new ProductTaggedEntitiesService(_itemService);
            }
            else
            {
                throw new NotSupportedException(entityType);
            }
            return result;
        }
    }
}
