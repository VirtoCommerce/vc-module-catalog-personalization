using System.Collections.Generic;
using System.Linq;
using Moq;
using VirtoCommerce.CatalogModule.Core.Model.Search;
using VirtoCommerce.CatalogPersonalizationModule.Data.Search;
using VirtoCommerce.SearchModule.Core.Model;
using VirtoCommerce.SearchModule.Core.Services;
using Xunit;

namespace VirtoCommerce.CatalogPersonalizationModule.Test.Indexations
{
    public class CategorySearchUserGroupsRequestBuilderTests
    {
        [Theory]
        [MemberData(nameof(Data))]
        public void ProductSearchUserGroupsRequestBuilder_GetPermanentFiltersTest(List<string> userGroups, List<string> result)
        {
            // Arrange
            var parser = new Mock<ISearchPhraseParser>();

            var target = new CategorySearchUserGroupsRequestBuilderStub(parser.Object);

            // Act
            var criteria = new CategoryIndexedSearchCriteria()
            {
                UserGroups = userGroups,
            };
            var filters = target.GetFiltersStub(criteria);

            // Assert
            var userGroupsFilters = filters.OfType<TermFilter>().FirstOrDefault(x => x.FieldName == "user_groups");
            Assert.Equal(userGroupsFilters.Values, result);
        }

        public static IEnumerable<object[]> Data =>
            new List<object[]>
            {
                // returns exact user groups
                new object[] { new List<string> { "VIP" }, new List<string> { "VIP" } },
                // returns "__any" keyword for empty user groups criteria
                new object[] { new List<string>(), new List<string> { "__any" } },
            };


        public class CategorySearchUserGroupsRequestBuilderStub : CategorySearchUserGroupsRequestBuilder
        {
            public CategorySearchUserGroupsRequestBuilderStub(ISearchPhraseParser searchPhraseParser)
                : base(searchPhraseParser)
            {
            }

            public IList<IFilter> GetFiltersStub(CategoryIndexedSearchCriteria criteria) => base.GetFilters(criteria);
        }
    }
}
