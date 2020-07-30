using System;
using System.Collections.Generic;
using VirtoCommerce.CatalogModule.Core.Model;
using VirtoCommerce.CatalogPersonalizationModule.Core.Model;
using VirtoCommerce.CoreModule.Core.Outlines;
using Xunit;

namespace VirtoCommerce.CatalogPersonalizationModule.Test.Indexations
{
    [Trait("Category", "CI")]
    public class ProductDocumentIndexationTests : IndexationTestsBase
    {
        //[Theory]
        //[MemberData(nameof(TestDocumentsDataGenerator.GetData), MemberType = typeof(TestDocumentsDataGenerator))]
        //public async Task TestProductDocumentBuilder(TestDocumentInputData inputData)
        //{
        //    var documentBuilder = new TaggedItemProductDocumentBuilder(GetItemService(inputData.AllProducts), GetTaggedItemService(inputData.TaggedItems));
        //    var products = inputData.Products;
        //    var productIds = products.Select(p => p.Id).ToList();

        //    var documents = await documentBuilder.GetDocumentsAsync(productIds);

        //    Assert.Equal(documents.Count, products.Count);

        //    foreach (var product in products)
        //    {
        //        var document = documents.FirstOrDefault(d => d.Id.EqualsInvariant(product.Id));
        //        Assert.NotNull(document);
        //        Assert.Equal(1, document.Fields.Count);

        //        var field = document.Fields.First();
        //        Assert.Equal(field.Values.Count, inputData.TagsCount);
        //    }
        //}
    }

    public class TestDocumentInputData
    {
        public IList<CatalogProduct> Products { get; set; }
        public IList<CatalogProduct> AllProducts { get; set; }
        public IList<TaggedItem> TaggedItems { get; set; }
        public int TagsCount { get; set; }
    }

    public class TestDocumentsDataGenerator
    {
        private const string _catalogIdOneTag = "catalog-1";
        private const string _catalogIdTwoTags = "catalog-2";

        private const string _productIdOneTag = "product-1";
        private const string _productIdTwoTags = "product-2";
        private const string _productIdThreeTagsWithTwoIdentical = "product-3";
        private const string _productIdOneTagAndCatalogOneTag = "product-4";

        public static IEnumerable<object[]> GetData()
        {
            //Product: 1 tag, catalog: 1 tag, result: 2 tags
            yield return new object[]
            {
                new TestDocumentInputData
                {
                    Products = new List<CatalogProduct> { _productWithOneTag },
                    AllProducts = _allProducts,
                    TaggedItems = _allTaggedItems,
                    TagsCount = 2
                }
            };

            //Product: 2 tags, catalog: 2 tags, result: 4 tags
            yield return new object[]
            {
                new TestDocumentInputData
                {
                    Products = new List<CatalogProduct> { _productWithTwoTags },
                    AllProducts = _allProducts,
                    TaggedItems = _allTaggedItems,
                    TagsCount = 4
                }
            };

            //Product: 3 tags with 2 unique, catalog: 2 tags, result: 4 tags
            yield return new object[]
            {
                new TestDocumentInputData
                {
                    Products = new List<CatalogProduct> { _productWithThreeTagsWithTwoIdentical },
                    AllProducts = _allProducts,
                    TaggedItems = _allTaggedItems,
                    TagsCount = 4
                }
            };

            //Product: 2 tags and 1 same as in catalog, catalog: 1 tag, result: 2 tags
            yield return new object[]
            {
                new TestDocumentInputData
                {
                    Products = new List<CatalogProduct> { _productWithOneTagAndCatalogOneTag },
                    AllProducts = _allProducts,
                    TaggedItems = _allTaggedItems,
                    TagsCount = 2
                }
            };

            //Product: no tags, catalog: no tags, result: 1 tag ("__any")
            yield return new object[]
            {
                new TestDocumentInputData
                {
                    Products = new List<CatalogProduct> { _productWithoutTags },
                    AllProducts = _allProducts,
                    TaggedItems = _allTaggedItems,
                    TagsCount = 1
                }
            };
        }

        //Product: 1 tag, catalog: 1 tag
        private static CatalogProduct _productWithOneTag = new CatalogProduct
        {
            Id = _productIdOneTag,
            Outlines = new List<Outline>
            {
                new Outline
                {
                    Items = new List<OutlineItem>
                    {
                        new OutlineItem { Id = _catalogIdOneTag },
                        new OutlineItem { Id = _productIdOneTag }
                    }
                }
            }
        };

        //Product: 2 tags, catalog: 2 tags
        private static CatalogProduct _productWithTwoTags = new CatalogProduct
        {
            Id = _productIdTwoTags,
            Outlines = new List<Outline>
            {
                new Outline
                {
                    Items = new List<OutlineItem>
                    {
                        new OutlineItem { Id = _catalogIdTwoTags },
                        new OutlineItem { Id = _productIdTwoTags }
                    }
                }
            }
        };

        //Product: 2 tags, catalog: 2 tags
        private static CatalogProduct _productWithThreeTagsWithTwoIdentical = new CatalogProduct
        {
            Id = _productIdThreeTagsWithTwoIdentical,
            Outlines = new List<Outline>
            {
                new Outline
                {
                    Items = new List<OutlineItem>
                    {
                        new OutlineItem { Id = _catalogIdTwoTags },
                        new OutlineItem { Id = _productIdThreeTagsWithTwoIdentical }
                    }
                }
            }
        };

        //Product: 1 tag, catalog: 1 tag
        private static CatalogProduct _productWithOneTagAndCatalogOneTag = new CatalogProduct
        {
            Id = _productIdOneTagAndCatalogOneTag,
            Outlines = new List<Outline>
            {
                new Outline
                {
                    Items = new List<OutlineItem>
                    {
                        new OutlineItem { Id = _catalogIdOneTag },
                        new OutlineItem { Id = _productIdOneTagAndCatalogOneTag }
                    }
                }
            }
        };

        //Product: 0 tag, catalog: 0 tag
        private static CatalogProduct _productWithoutTags = new CatalogProduct
        {
            Id = Guid.NewGuid().ToString(),
            Outlines = new List<Outline>
            {
                new Outline
                {
                    Items = new List<OutlineItem>
                    {
                        new OutlineItem { Id = Guid.NewGuid().ToString() },
                        new OutlineItem { Id = Guid.NewGuid().ToString() }
                    }
                }
            }
        };

        private static CatalogProduct[] _allProducts =
        {
            _productWithOneTag,
            _productWithTwoTags,
            _productWithThreeTagsWithTwoIdentical,
            _productWithOneTagAndCatalogOneTag,
            _productWithoutTags
        };

        private static TaggedItem[] _allTaggedItems =
        {
            new TaggedItem
            {
                EntityId = _catalogIdOneTag, Tags = new[] { "tag-catalog-1" }
            },
            new TaggedItem
            {
                EntityId = _catalogIdTwoTags, Tags = new[] { "tag-catalog-2.1", "tag-catalog-2.2" }
            },
            new TaggedItem
            {
                EntityId = _productIdOneTag, Tags = new[] { "tag-product-1" }
            },
            new TaggedItem
            {
                EntityId = _productIdTwoTags, Tags = new[] { "tag-product-2.1", "tag-product-2.2" }
            },
            new TaggedItem
            {
                EntityId = _productIdThreeTagsWithTwoIdentical, Tags = new[] { "tag-product-3.1", "tag-product-3.1", "tag-product-3.2" }
            },
            new TaggedItem
            {
                EntityId = _productIdOneTagAndCatalogOneTag, Tags = new[] { "tag-product-4", "tag-catalog-1" }
            }
        };
    }
}
