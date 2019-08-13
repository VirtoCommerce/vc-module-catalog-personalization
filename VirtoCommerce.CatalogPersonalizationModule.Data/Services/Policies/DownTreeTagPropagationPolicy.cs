using System;
using System.Collections.Generic;
using System.Linq;
using VirtoCommerce.CatalogPersonalizationModule.Core.Services;
using VirtoCommerce.CatalogPersonalizationModule.Data.Repositories;
using VirtoCommerce.Domain.Catalog.Model;
using VirtoCommerce.Platform.Core.Common;
using VirtoCommerce.Platform.Data.Infrastructure;

namespace VirtoCommerce.CatalogPersonalizationModule.Data.Services
{
    public class DownTreeTagPropagationPolicy : ITagPropagationPolicy
    {
        private readonly Func<IPersonalizationRepository> _repositoryFactory;
        private readonly ITaggedItemService _taggedItemService;

        public DownTreeTagPropagationPolicy(Func<IPersonalizationRepository> repositoryFactory, ITaggedItemService taggedItemService)
        {
            _repositoryFactory = repositoryFactory;
            _taggedItemService = taggedItemService;
        }

        public Dictionary<string, HashSet<string>> GetResultingTags(IEntity[] entities)
        {
            var result = entities.ToDictionary(x => x.Id, x => new HashSet<string>());

            var entitiesIds = entities.Select(x => x.Id).ToArray();
            using (var repository = _repositoryFactory())
            {
                repository.DisableChangesTracking();

                //Loading own tags for given entities
                var taggedItemIds = repository.TaggedItems.Where(x => entitiesIds.Contains(x.ObjectId))
                                                          .Select(x => x.Id)
                                                          .ToArray();
                var taggedItems = _taggedItemService.GetTaggedItemsByIds(taggedItemIds);
                foreach (var taggedItem in taggedItems)
                {
                    result[taggedItem.EntityId].AddRange(taggedItem.Tags);
                }

                var allOutlineItemIds = entities.OfType<IHasOutlines>().Where(x => !x.Outlines.IsNullOrEmpty())
                                                .SelectMany(x => x.Outlines.SelectMany(o => o.Items))
                                                .Select(x => x.Id).Distinct(StringComparer.OrdinalIgnoreCase)
                                                .ToArray();

                taggedItemIds = repository.TaggedItems.Where(x => allOutlineItemIds.Contains(x.ObjectId)).Select(x => x.Id).ToArray();
                taggedItems = _taggedItemService.GetTaggedItemsByIds(taggedItemIds);
                foreach(var entity in entities)
                {
                    if (entity is IHasOutlines hasOulines)
                    {
                        var outlineItemsIds = hasOulines.Outlines.SelectMany(x => x.Items)
                                                          .Select(x => x.Id)
                                                          .Distinct(StringComparer.OrdinalIgnoreCase)
                                                          .ToArray();
                        result[entity.Id].AddRange(taggedItems.Where(x => outlineItemsIds.Contains(x.EntityId)).SelectMany(x => x.Tags));
                    }
                }
            }
            return result;
        }
    }
}
