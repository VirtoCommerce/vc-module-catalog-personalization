using System;
using System.Linq;
using System.Threading.Tasks;
using VirtoCommerce.CatalogPersonalizationModule.Core.Model;
using VirtoCommerce.CatalogPersonalizationModule.Data.Repositories;
using VirtoCommerce.Platform.Core.Common;
using VirtoCommerce.Platform.Data.Infrastructure;

namespace VirtoCommerce.CatalogPersonalizationModule.Data.Services
{
    public class TreeTagPropagationPolicy
    {
        private readonly Func<IPersonalizationRepository> _repositoryFactory;

        public TreeTagPropagationPolicy(Func<IPersonalizationRepository> repositoryFactory)
        {
            _repositoryFactory = repositoryFactory;
        }

        protected async Task<TaggedItem[]> GetTaggedItemsByIdsAsync(string[] ids, string responseGroup = null)
        {
            TaggedItem[] result = null;
            if (ids != null)
            {
                using (var repository = _repositoryFactory())
                {
                    repository.DisableChangesTracking();
                    var taggedItems = await repository.GetTaggedItemsByIdsAsync(ids, responseGroup);
                    result = taggedItems.Select(x => x.ToModel(AbstractTypeFactory<TaggedItem>.TryCreateInstance())).ToArray();
                }
            }
            return result;
        }

    }
}
