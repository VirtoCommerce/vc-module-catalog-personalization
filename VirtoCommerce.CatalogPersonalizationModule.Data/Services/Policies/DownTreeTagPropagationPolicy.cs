using System;
using System.Collections.Generic;
using System.Linq;
using VirtoCommerce.CatalogPersonalizationModule.Core.Model;
using VirtoCommerce.CatalogPersonalizationModule.Core.Services;
using VirtoCommerce.CatalogPersonalizationModule.Data.Repositories;
using VirtoCommerce.Domain.Catalog.Model;
using VirtoCommerce.Platform.Core.Common;
using VirtoCommerce.Platform.Data.Infrastructure;

namespace VirtoCommerce.CatalogPersonalizationModule.Data.Services
{
    public class DownTreeTagPropagationPolicy : TreeTagPropagationPolicy, ITagPropagationPolicy
    {
        private readonly Func<IPersonalizationRepository> _repositoryFactory;

        public DownTreeTagPropagationPolicy(Func<IPersonalizationRepository> repositoryFactory) : base(repositoryFactory)
        {
            _repositoryFactory = repositoryFactory;
        }

        public Dictionary<string, List<EffectiveTag>> GetResultingTags(IEntity[] entities)
        {
            var result = entities.ToDictionary(x => x.Id, x => new List<EffectiveTag>());

            var entitiesIds = entities.Select(x => x.Id).ToArray();
            using (var repository = _repositoryFactory())
            {
                repository.DisableChangesTracking();

                //Loading own tags for given entities
                var taggedItemIds = repository.TaggedItems.Where(x => entitiesIds.Contains(x.ObjectId))
                                                          .Select(x => x.Id)
                                                          .ToArray();
                var taggedItems = GetTaggedItemsByIds(taggedItemIds);
                foreach (var taggedItem in taggedItems)
                {
                    result[taggedItem.EntityId].AddRange(taggedItem.Tags.Select(x => EffectiveTag.NonInheritedTag(x)));
                }

                var allOutlineItemIds = entities.OfType<IHasOutlines>().Where(x => !x.Outlines.IsNullOrEmpty())
                                                .SelectMany(x => x.Outlines.SelectMany(o => o.Items))
                                                .Select(x => x.Id).Distinct(StringComparer.OrdinalIgnoreCase)
                                                .ToArray();

                taggedItemIds = repository.TaggedItems.Where(x => allOutlineItemIds.Contains(x.ObjectId)).Select(x => x.Id).ToArray();
                taggedItems = GetTaggedItemsByIds(taggedItemIds);
                foreach (var entity in entities)
                {
                    if (entity is IHasOutlines hasOulines)
                    {
                        var outlineItemsIds = hasOulines.Outlines.SelectMany(x => x.Items)
                                                          .Select(x => x.Id)
                                                          .Distinct(StringComparer.OrdinalIgnoreCase)
                                                          .ToArray();
                        result[entity.Id].AddRange(taggedItems
                            .Where(x => outlineItemsIds
                            .Contains(x.EntityId))
                            .SelectMany(x => x.Tags
                                .Select(y => EffectiveTag.InheritedTag(y))));

                        // Need to remove duplicates
                        result[entity.Id] = result[entity.Id].Distinct(AnonymousComparer.Create<EffectiveTag, string>(x => $"{x.Tag}:{x.IsInherited}")).ToList();
                    }
                }
            }
            return result;
        }
    }
}
