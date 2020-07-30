using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using VirtoCommerce.CatalogPersonalizationModule.Core.Model;
using VirtoCommerce.CatalogPersonalizationModule.Core.Services;
using VirtoCommerce.CoreModule.Core.Outlines;
using VirtoCommerce.Platform.Core.Common;

namespace VirtoCommerce.CatalogPersonalizationModule.Data.Services
{
    public class TaggedItemOutlinesSynchronizer : ITaggedItemOutlinesSynchronizer
    {
        private readonly ITaggedItemService _taggedItemService;
        private readonly ITaggedEntitiesServiceFactory _taggedEntriesServiceFactory;
        public TaggedItemOutlinesSynchronizer(ITaggedItemService tagItemService, ITaggedEntitiesServiceFactory taggedEntriesServiceFactory)
        {
            _taggedItemService = tagItemService;
            _taggedEntriesServiceFactory = taggedEntriesServiceFactory;
        }

        public async Task SynchronizeOutlinesAsync(TaggedItem[] taggedItems)
        {
            if (taggedItems == null)
            {
                throw new ArgumentNullException(nameof(taggedItems));
            }

            var groupByEntityType = taggedItems.GroupBy(x => x.EntityType);
            foreach (var group in groupByEntityType)
            {
                var toRemoveTaggedItemIds = new List<string>();
                //Create appropriate service for load entities depends on their type 
                var entitiesService = _taggedEntriesServiceFactory.Create(group.Key);

                var taggedEntities = (await entitiesService.GetEntitiesByIdsAsync(group.Select(x => x.EntityId).ToArray())).Distinct().ToArray();
                foreach (var taggedItem in group)
                {
                    var referencedEntity = taggedEntities.FirstOrDefault(x => x.Id.EqualsInvariant(taggedItem.EntityId));
                    if (referencedEntity == null)
                    {
                        toRemoveTaggedItemIds.Add(taggedItem.Id);
                    }
                    else if (referencedEntity is IHasOutlines hasOutlines)
                    {
                        taggedItem.Outlines = hasOutlines.Outlines.Select(x => x.ToString()).Select(x => new TaggedItemOutline { Outline = x }).ToList();
                    }
                }

                await _taggedItemService.SaveChangesAsync(@group.ToArray());

                if (toRemoveTaggedItemIds.Any())
                {
                    await _taggedItemService.DeleteAsync(toRemoveTaggedItemIds.ToArray());
                }
            }
        }
    }
}
