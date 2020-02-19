using System;
using System.Linq;
using System.Threading.Tasks;
using Moq;
using VirtoCommerce.CatalogPersonalizationModule.Core.Model;
using VirtoCommerce.CatalogPersonalizationModule.Core.Model.Search;
using VirtoCommerce.CatalogPersonalizationModule.Core.Services;
using VirtoCommerce.CatalogPersonalizationModule.Data.Search.Indexing;
using VirtoCommerce.Platform.Core.ChangeLog;
using VirtoCommerce.Platform.Core.Common;
using VirtoCommerce.SearchModule.Core.Model;
using Xunit;

namespace VirtoCommerce.CatalogPersonalizationModule.Test.Indexations
{
    [Trait("Category", "CI")]
    public class TaggedItemIndexChangesProviderTests
    {
        private readonly DateTime _startIndexDateTime = DateTime.Parse("5/11/2017 12:00 PM");
        private readonly DateTime _endIndexDateTime = DateTime.Parse("5/12/2017 12:00 AM");

        private const string _catalogId = "catalog-id";
        private const string _product1Id = "product-1";
        private const string _product2Id = "product-2";

        private static TaggedItem[] _allTaggedItems =
        {
            new TaggedItem
            {
                Id = _catalogId, EntityId = "First"
            },
            new TaggedItem
            {
                Id = _product1Id, EntityId = "Second"
            },
            new TaggedItem
            {
                Id = _product2Id, EntityId = "Third"
            },
            new TaggedItem
            {
                Id = Guid.NewGuid().ToString(), EntityId = Guid.NewGuid().ToString()
            },
            new TaggedItem
            {
                Id = Guid.NewGuid().ToString(), EntityId = Guid.NewGuid().ToString()
            }
        };

        [Fact]
        public async Task TestOperationProvider()
        {
            var changesProvider = new TaggedItemIndexChangesProvider(GetTaggedItemSearchService(), GetChangeLogSearchService());

            var changesCount = await changesProvider.GetTotalChangesCountAsync(_startIndexDateTime, _endIndexDateTime);
            Assert.Equal(4, changesCount);

            var changes = await changesProvider.GetChangesAsync(_startIndexDateTime, _endIndexDateTime, 0, changesCount);
            Assert.Collection(changes,
                c => Assert.True(c.DocumentId == "First" && c.ChangeDate == DateTime.Parse("5/11/2017 1:00 PM") && c.ChangeType == IndexDocumentChangeType.Modified),
                c => Assert.True(c.DocumentId == "Second" && c.ChangeDate == DateTime.Parse("5/11/2017 2:00 PM") && c.ChangeType == IndexDocumentChangeType.Modified),
                c => Assert.True(c.DocumentId == "Third" && c.ChangeDate == DateTime.Parse("5/11/2017 3:00 PM") && c.ChangeType == IndexDocumentChangeType.Modified));
        }

        protected ITaggedItemSearchService GetTaggedItemSearchService()
        {
            var service = new Mock<ITaggedItemSearchService>();
            service.Setup(x => x.SearchTaggedItemsAsync(It.IsAny<TaggedItemSearchCriteria>()))
                .ReturnsAsync((TaggedItemSearchCriteria criteria) => new TaggedItemSearchResult { Results = _allTaggedItems.Where(p => criteria.Ids.Contains(p.Id)).ToArray() });
            return service.Object;
        }

        private IChangeLogSearchService GetChangeLogSearchService()
        {
            var searchService = new Mock<IChangeLogSearchService>();
            searchService.Setup(x => x.SearchAsync(It.IsAny<ChangeLogSearchCriteria>()))
                .ReturnsAsync(() => new ChangeLogSearchResult()
                {
                    TotalCount = 4,
                    Results =
                    {
                        new OperationLog
                        {
                            Id = Guid.NewGuid().ToString(),
                            CreatedDate = DateTime.Parse("5/11/2017 1:00 PM"),
                            CreatedBy = "Test",
                            ObjectType = "TaggedItemEntity",
                            ObjectId = _catalogId,
                            OperationType = EntryState.Added
                        },
                        new OperationLog
                        {
                            Id = Guid.NewGuid().ToString(),
                            CreatedDate = DateTime.Parse("5/11/2017 2:00 PM"),
                            CreatedBy = "Test",
                            ObjectType = "TaggedItemEntity",
                            ObjectId = _product1Id,
                            OperationType = EntryState.Added
                        },
                        new OperationLog
                        {
                            Id = Guid.NewGuid().ToString(),
                            CreatedDate = DateTime.Parse("5/11/2017 2:00 PM"),
                            CreatedBy = "Test",
                            ModifiedDate = DateTime.Parse("5/11/2017 3:00 PM"),
                            ModifiedBy = "Test",
                            ObjectType = "TaggedItemEntity",
                            ObjectId = _product2Id,
                            OperationType = EntryState.Modified
                        },
                        new OperationLog
                        {
                            Id = Guid.NewGuid().ToString(),
                            CreatedDate = DateTime.Parse("5/11/2017 4:00 PM"),
                            CreatedBy = "Test",
                            ObjectType = "TaggedItemEntity",
                            ObjectId = "Fourth",
                            OperationType = EntryState.Added
                        }
                    }
                });
            return searchService.Object;
        }
    }
}
