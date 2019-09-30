using System;
using System.Linq;
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

        protected TaggedItem[] GetTaggedItemsByIds(string[] ids, string responseGroup = null)
        {
            TaggedItem[] retVal = null;
            if (ids != null)
            {
                using (var repository = _repositoryFactory())
                {
                    repository.DisableChangesTracking();

                    retVal = repository.GetTaggedItemsByIds(ids, responseGroup).Select(x => x.ToModel(AbstractTypeFactory<TaggedItem>.TryCreateInstance())).ToArray();
                }
            }
            return retVal;
        }

    }
}
