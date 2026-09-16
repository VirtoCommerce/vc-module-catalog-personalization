using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using MockQueryable;
using Moq;
using VirtoCommerce.CatalogModule.Core.Model;
using VirtoCommerce.CatalogModule.Core.Outlines;
using VirtoCommerce.CatalogPersonalizationModule.Data.Model;
using VirtoCommerce.CatalogPersonalizationModule.Data.Repositories;
using VirtoCommerce.CatalogPersonalizationModule.Data.Services;
using VirtoCommerce.Platform.Core.Common;
using Xunit;

namespace VirtoCommerce.CatalogPersonalizationModule.Test.Policies
{
    [Trait("Category", "CI")]
    public class DownTreeTagPropagationPolicyTests
    {
        private const string CatalogId = "catalog-1";
        private const string ParentCategoryId = "category-parent";
        private const string CategoryId = "category-1";

        [Fact]
        public async Task GetResultingTags_TopLevelEntity_DoesNotInheritItsOwnTags()
        {
            // An outline always ends with the entity itself, which used to make every directly
            // assigned tag come back as inherited as well.
            var category = CreateCategory(CategoryId, CatalogId);
            var policy = CreatePolicy(CreateTaggedItem("ti-1", CategoryId, "Retail", "VIP"));

            var result = await policy.GetResultingTagsAsync(new[] { category });

            var tags = result[CategoryId];
            Assert.Equal(new[] { "Retail", "VIP" }, tags.Where(x => !x.IsInherited).Select(x => x.Tag).OrderBy(x => x));
            Assert.DoesNotContain(tags, x => x.IsInherited);
        }

        [Fact]
        public async Task GetResultingTags_AncestorTags_AreStillInherited()
        {
            var category = CreateCategory(CategoryId, CatalogId, ParentCategoryId);
            var policy = CreatePolicy(
                CreateTaggedItem("ti-1", CategoryId, "Retail"),
                CreateTaggedItem("ti-2", ParentCategoryId, "Wholesale"));

            var result = await policy.GetResultingTagsAsync(new[] { category });

            var tags = result[CategoryId];
            Assert.Equal(new[] { "Retail" }, tags.Where(x => !x.IsInherited).Select(x => x.Tag));
            Assert.Equal(new[] { "Wholesale" }, tags.Where(x => x.IsInherited).Select(x => x.Tag));
        }

        [Fact]
        public async Task GetResultingTags_TagAssignedDirectlyAndByAncestor_IsBothDirectAndInherited()
        {
            var category = CreateCategory(CategoryId, CatalogId, ParentCategoryId);
            var policy = CreatePolicy(
                CreateTaggedItem("ti-1", CategoryId, "Retail"),
                CreateTaggedItem("ti-2", ParentCategoryId, "Retail"));

            var result = await policy.GetResultingTagsAsync(new[] { category });

            var tags = result[CategoryId];
            Assert.Single(tags, x => !x.IsInherited && x.Tag == "Retail");
            Assert.Single(tags, x => x.IsInherited && x.Tag == "Retail");
        }

        private static DownTreeTagPropagationPolicy CreatePolicy(params TaggedItemEntity[] taggedItems)
        {
            var repository = new Mock<IPersonalizationRepository>();
            repository.Setup(x => x.TaggedItems).Returns(taggedItems.BuildMock());
            repository.Setup(x => x.GetTaggedItemsByIdsAsync(It.IsAny<string[]>(), It.IsAny<string>()))
                .ReturnsAsync((string[] ids, string _) => taggedItems.Where(x => ids.Contains(x.Id)).ToArray());

            return new DownTreeTagPropagationPolicy(() => repository.Object);
        }

        private static Category CreateCategory(string id, params string[] ancestorIds)
        {
            var items = ancestorIds.Select(x => new OutlineItem { Id = x }).ToList();
            items.Add(new OutlineItem { Id = id });

            return new Category
            {
                Id = id,
                Outlines = new List<Outline> { new Outline { Items = items } }
            };
        }

        private static TaggedItemEntity CreateTaggedItem(string id, string objectId, params string[] tags)
        {
            return new TaggedItemEntity
            {
                Id = id,
                ObjectId = objectId,
                ObjectType = nameof(Category),
                Label = objectId,
                Tags = new ObservableCollection<TagEntity>(
                    tags.Select(x => new TagEntity { Tag = x, TaggedItemId = id }))
            };
        }
    }
}
