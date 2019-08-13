using VirtoCommerce.CatalogPersonalizationModule.Core.Services;
using VirtoCommerce.Domain.Search;

namespace VirtoCommerce.CatalogPersonalizationModule.Data.Search.Indexing
{
    public class CategoryTaggedItemDocumentBuilder : TaggedItemDocumentBuilderBase
    {
        public CategoryTaggedItemDocumentBuilder(ITagPropagationPolicy tagInheritancePolicy, ITaggedEntitiesServiceFactory taggedEntitiesServiceFactory)
            : base(tagInheritancePolicy, taggedEntitiesServiceFactory)
        {
        }
        protected override string DocumentTypeName => KnownDocumentTypes.Category;
    }
}
