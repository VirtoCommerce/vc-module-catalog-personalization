using VirtoCommerce.CatalogPersonalizationModule.Core.Services;
using VirtoCommerce.Domain.Search;

namespace VirtoCommerce.CatalogPersonalizationModule.Data.Search.Indexing
{
    public class ProductTaggedItemDocumentBuilder : TaggedItemDocumentBuilderBase
    {
        public ProductTaggedItemDocumentBuilder(ITagPropagationPolicy tagInheritancePolicy, ITaggedEntitiesServiceFactory taggedEntitiesServiceFactory)
            : base(tagInheritancePolicy, taggedEntitiesServiceFactory)
        {
        }
        protected override string DocumentTypeName => KnownDocumentTypes.Product;
    }
}
