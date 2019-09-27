using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using VirtoCommerce.CatalogPersonalizationModule.Core.Services;
using VirtoCommerce.CatalogPersonalizationModule.Data.Common;
using VirtoCommerce.Domain.Search;
using VirtoCommerce.Platform.Core.Common;

namespace VirtoCommerce.CatalogPersonalizationModule.Data.Search.Indexing
{
    public abstract class TaggedItemDocumentBuilderBase : IIndexDocumentBuilder
    {
        private readonly ITagPropagationPolicy _tagInheritancePolicy;
        private readonly ITaggedEntitiesServiceFactory _taggedEntitiesServiceFactory;

        protected TaggedItemDocumentBuilderBase(ITagPropagationPolicy tagInheritancePolicy, ITaggedEntitiesServiceFactory taggedEntitiesServiceFactory)
        {
            _tagInheritancePolicy = tagInheritancePolicy;
            _taggedEntitiesServiceFactory = taggedEntitiesServiceFactory;
        }

        protected abstract string DocumentTypeName { get; }

        public Task<IList<IndexDocument>> GetDocumentsAsync(IList<string> documentIds)
        {
            IList<IndexDocument> result = new List<IndexDocument>();

            var entities = _taggedEntitiesServiceFactory.Create(DocumentTypeName).GetEntitiesByIds(documentIds.ToArray());

            var lookup = _tagInheritancePolicy.GetResultingTags(entities);
            foreach (var keyValue in lookup)
            {
                var tags = keyValue.Value.Select(x => x.Tag).Distinct().ToArray();
                var document = CreateDocument(keyValue.Key, tags);
                result.Add(document);
            }
            return Task.FromResult(result);
        }


        protected virtual IndexDocument CreateDocument(string id, string[] tags)
        {
            var document = new IndexDocument(id);

            if (tags.IsNullOrEmpty())
            {
                tags = new[] { Constants.UserGroupsAnyValue };
            }
            foreach (var tag in tags)
            {
                document.Add(new IndexDocumentField(Constants.UserGroupsFieldName, tag) { IsRetrievable = true, IsFilterable = true, IsCollection = true });
            }

            return document;
        }
    }
}
