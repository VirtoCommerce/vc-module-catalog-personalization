using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using VirtoCommerce.CatalogPersonalizationModule.Core.Model;
using VirtoCommerce.CatalogPersonalizationModule.Data.Search.Indexing;
using VirtoCommerce.Domain.Catalog.Model;
using VirtoCommerce.Platform.Core.Common;
using Xunit;

namespace VirtoCommerce.CatalogPersonalizationModule.Test.Indexations
{
	[Trait("Category", "CI")]
	public class CategoryDocumentIndexationTests : IndexationTestsBase
	{
		//[Theory]
		//[MemberData(nameof(TestCategoriesDataGenerator.GetData), MemberType = typeof(TestCategoriesDataGenerator))]
		//public async Task TestCategoryDocumentBuilder(TestCategoryInputData inputData)
		//{
		//	var documentBuilder = new TaggedItemCategoryDocumentBuilder(GetCategoryService(inputData.AllCategories), GetTaggedItemService(inputData.TaggedItems), GetTaggedItemOutlineService(new List<TaggedItemOutline>()));
		//	var categories = inputData.Categories;
		//	var categoryIds = categories.Select(p => p.Id).ToList();

		//	var documents = await documentBuilder.GetDocumentsAsync(categoryIds);

		//	Assert.Equal(documents.Count, categories.Count);

		//	foreach (var category in categories)
		//	{
		//		var document = documents.FirstOrDefault(d => d.Id.EqualsInvariant(category.Id));
		//		Assert.NotNull(document);
		//		Assert.Equal(1, document.Fields.Count);

		//		var field = document.Fields.First();
		//		Assert.Equal(field.Values.Count, inputData.TagsCount);
		//	}
		//}
	}

	public class TestCategoryInputData
	{
		public IList<Category> Categories { get; set; }
		public IList<Category> AllCategories { get; set; }
		public IList<TaggedItem> TaggedItems { get; set; }
		public int TagsCount { get; set; }
	}

	public class TestCategoriesDataGenerator
	{
		public static IEnumerable<object[]> GetData()
		{
			//Product: 1 tag, catalog: 1 tag, result: 2 tags
			yield return new object[]
			{
				new TestCategoryInputData
				{
					Categories = new List<Category> { _categoryWithOneTag },
					AllCategories = _allCategories,
					TaggedItems = _allTaggedItems,
					TagsCount = 2
				}
			};

			//Product: 2 tags, catalog: 2 tags, result: 4 tags
			yield return new object[]
			{
				new TestCategoryInputData
				{
					Categories = new List<Category> { _categoryWithTwoTags },
					AllCategories = _allCategories,
					TaggedItems = _allTaggedItems,
					TagsCount = 4
				}
			};

			//Product: 3 tags with 2 unique, catalog: 2 tags, result: 4 tags
			yield return new object[]
			{
				new TestCategoryInputData
				{
					Categories = new List<Category> { _categoryWithThreeTagsWithTwoIdentical },
					AllCategories = _allCategories,
					TaggedItems = _allTaggedItems,
					TagsCount = 4
				}
			};

			//Product: 2 tags and 1 same as in catalog, catalog: 1 tag, result: 2 tags
			yield return new object[]
			{
				new TestCategoryInputData
				{
					Categories = new List<Category> { _categoryWithOneTagAndCatalogOneTag },
					AllCategories = _allCategories,
					TaggedItems = _allTaggedItems,
					TagsCount = 2
				}
			};
		}

		private const string _catalogIdOneTag = "catalog-1";
		private const string _catalogIdTwoTags = "catalog-2";

		private const string _categoryIdOneTag = "category-1";
		private const string _categoryIdTwoTags = "category-2";
		private const string _categoryIdThreeTagsWithTwoIdentical = "category-3";
		private const string _categoryIdOneTagAndCatalogOneTag = "category-4";

		//Product: 1 tag, catalog: 1 tag
		private static Category _categoryWithOneTag = new Category
		{
			Id = _categoryIdOneTag,
			Outlines = new List<Outline>
			{
				new Outline
				{
					Items = new List<OutlineItem>
					{
						new OutlineItem { Id = _catalogIdOneTag },
						new OutlineItem { Id = _categoryIdOneTag }
					}
				}
			}
		};

		//Product: 2 tags, catalog: 2 tags
		private static Category _categoryWithTwoTags = new Category
		{
			Id = _categoryIdTwoTags,
			Outlines = new List<Outline>
			{
				new Outline
				{
					Items = new List<OutlineItem>
					{
						new OutlineItem { Id = _catalogIdTwoTags },
						new OutlineItem { Id = _categoryIdTwoTags }
					}
				}
			}
		};

		//Product: 2 tags, catalog: 2 tags
		private static Category _categoryWithThreeTagsWithTwoIdentical = new Category
		{
			Id = _categoryIdThreeTagsWithTwoIdentical,
			Outlines = new List<Outline>
			{
				new Outline
				{
					Items = new List<OutlineItem>
					{
						new OutlineItem { Id = _catalogIdTwoTags },
						new OutlineItem { Id = _categoryIdThreeTagsWithTwoIdentical }
					}
				}
			}
		};

		//Product: 1 tag, catalog: 1 tag
		private static Category _categoryWithOneTagAndCatalogOneTag = new Category
		{
			Id = _categoryIdOneTagAndCatalogOneTag,
			Outlines = new List<Outline>
			{
				new Outline
				{
					Items = new List<OutlineItem>
					{
						new OutlineItem { Id = _catalogIdOneTag },
						new OutlineItem { Id = _categoryIdOneTagAndCatalogOneTag }
					}
				}
			}
		};

		private static Category[] _allCategories =
		{
			_categoryWithOneTag,
			_categoryWithTwoTags,
			_categoryWithThreeTagsWithTwoIdentical,
			_categoryWithOneTagAndCatalogOneTag
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
				EntityId = _categoryIdOneTag, Tags = new[] { "tag-category-1" }
			},
			new TaggedItem
			{
				EntityId = _categoryIdTwoTags, Tags = new[] { "tag-category-2.1", "tag-category-2.2" }
			},
			new TaggedItem
			{
				EntityId = _categoryIdThreeTagsWithTwoIdentical, Tags = new[] { "tag-category-3.1", "tag-category-3.1", "tag-category-3.2" }
			},
			new TaggedItem
			{
				EntityId = _categoryIdOneTagAndCatalogOneTag, Tags = new[] { "tag-category-4", "tag-catalog-1" }
			}
		};
	}
}
