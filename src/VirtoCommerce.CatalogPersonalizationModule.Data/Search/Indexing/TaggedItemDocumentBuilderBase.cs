using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using VirtoCommerce.CatalogPersonalizationModule.Core.Services;
using VirtoCommerce.CatalogPersonalizationModule.Data.Common;
using VirtoCommerce.Platform.Core.Common;
using VirtoCommerce.SearchModule.Core.Model;
using VirtoCommerce.SearchModule.Core.Services;

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

        public async Task<IList<IndexDocument>> GetDocumentsAsync(IList<string> documentIds)
        {
            IList<IndexDocument> result = new List<IndexDocument>();

            var entities = await _taggedEntitiesServiceFactory.Create(DocumentTypeName).GetEntitiesByIdsAsync(documentIds.ToArray());

            var lookup = await _tagInheritancePolicy.GetResultingTagsAsync(entities);
            foreach (var keyValue in lookup)
            {
                var tags = keyValue.Value.Select(x => x.Tag).Distinct().ToArray();
                var document = CreateDocument(keyValue.Key, tags);
                result.Add(document);
            }
            return result;
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
