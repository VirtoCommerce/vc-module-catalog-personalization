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
    public class UpTreeTagPropagationPolicy : ITagPropagationPolicy
    {
        private readonly Func<IPersonalizationRepository> _repositoryFactory;
        private readonly ITaggedItemService _taggedItemService;
        public UpTreeTagPropagationPolicy(Func<IPersonalizationRepository> repositoryFactory, ITaggedItemService taggedItemService)
        {
            _repositoryFactory = repositoryFactory;
            _taggedItemService = taggedItemService;
        }

        public Dictionary<string, HashSet<string>> GetResultingTags(IEntity[] entities)
        {
            if(entities == null)
            {
                throw new ArgumentNullException(nameof(entities));
            }

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
                //Trying to propagate  tags from children objects, use for this the saved outlines of tagged items 
                foreach (var category in entities.OfType<Category>())
                {
                    if (!category.Outlines.IsNullOrEmpty())
                    {
                        var outlines = category.Outlines.Select(x => x.ToString())
                                                   .Distinct(StringComparer.OrdinalIgnoreCase)
                                                   .ToArray();
                        //load all descendants tag items for this category. Use for this the stored outlines
                        taggedItemIds = repository.TaggedItemOutlines.Where(i => outlines.Any(o => i.Outline.StartsWith(o)))
                                                                       .Select(i => i.TaggedItem.Id)
                                                                       .Distinct()
                                                                       .ToArray();
                        taggedItems = _taggedItemService.GetTaggedItemsByIds(taggedItemIds);
                        result[category.Id].AddRange(taggedItems.SelectMany(x => x.Tags));
                    }
                }
            }
            return result;
        }
    }
}
