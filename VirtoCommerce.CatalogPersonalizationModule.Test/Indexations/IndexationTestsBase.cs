using System.Collections.Generic;
using System.Linq;
using Moq;
using VirtoCommerce.CatalogPersonalizationModule.Core.Model;
using VirtoCommerce.CatalogPersonalizationModule.Core.Services;
using VirtoCommerce.Domain.Catalog.Model;
using VirtoCommerce.Domain.Catalog.Services;

namespace VirtoCommerce.CatalogPersonalizationModule.Test.Indexations
{
    public abstract class IndexationTestsBase
    {
        protected ICategoryService GetCategoryService(IList<Category> categories)
        {
            var service = new Mock<ICategoryService>();
            service.Setup(x => x.GetByIds(It.IsAny<string[]>(), It.IsAny<CategoryResponseGroup>(), It.IsAny<string>()))
                .Returns<string[], CategoryResponseGroup, string>((ids, rg, c) => categories.Where(p => ids.Contains(p.Id)).ToArray());
            return service.Object;
        }

        protected IItemService GetItemService(IList<CatalogProduct> products)
        {
            var service = new Mock<IItemService>();
            service.Setup(x => x.GetByIds(It.IsAny<string[]>(), It.IsAny<ItemResponseGroup>(), It.IsAny<string>()))
                .Returns<string[], ItemResponseGroup, string>((ids, rg, c) => products.Where(p => ids.Contains(p.Id)).ToArray());
            return service.Object;
        }

        protected ITaggedItemService GetTaggedItemService(IList<TaggedItem> taggedItems)
        {
            var service = new Mock<ITaggedItemService>();
            service.Setup(x => x.GetTaggedItemsByObjectIds(It.IsAny<string[]>()))
                .Returns<string[]>((ids) => taggedItems.Where(p => ids.Contains(p.EntityId)).ToArray());
            return service.Object;
        }
    }
}
