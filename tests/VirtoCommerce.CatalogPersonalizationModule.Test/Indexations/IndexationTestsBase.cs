using Moq;
using System.Collections.Generic;
using System.Linq;
using VirtoCommerce.CatalogModule.Core.Model;
using VirtoCommerce.CatalogModule.Core.Services;

namespace VirtoCommerce.CatalogPersonalizationModule.Test.Indexations
{
    public abstract class IndexationTestsBase
    {
        //protected ICategoryService GetCategoryService(IList<Category> categories)
        //{
        //    var service = new Mock<ICategoryService>();
        //    service.Setup(x => x.GetByIdsAsync(It.IsAny<string[]>(), It.IsAny<CategoryResponseGroup>().ToString(), It.IsAny<string>()))
        //        .ReturnsAsync<string[], CategoryResponseGroup, string>((ids, rg, c) => categories.Where(p => ids.Contains(p.Id)).ToArray());
        //    return service.Object;
        //}

        //protected IItemService GetItemService(IList<CatalogProduct> products)
        //{
        //    var service = new Mock<IItemService>();
        //    service.Setup(x => x.GetByIdsAsync(It.IsAny<string[]>(), It.IsAny<ItemResponseGroup>().ToString(), It.IsAny<string>()))
        //        .ReturnsAsync<string[], ItemResponseGroup, string>((ids, rg, c) => products.Where(p => ids.Contains(p.Id)).ToArray());
        //    return service.Object;
        //}
    }
}
