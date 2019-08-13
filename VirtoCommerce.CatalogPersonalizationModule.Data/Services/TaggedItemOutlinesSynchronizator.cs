using System;
using System.Collections.Generic;
using System.Linq;
using VirtoCommerce.CatalogPersonalizationModule.Core.Model;
using VirtoCommerce.CatalogPersonalizationModule.Core.Services;
using VirtoCommerce.Domain.Catalog.Model;
using VirtoCommerce.Platform.Core.Common;

namespace VirtoCommerce.CatalogPersonalizationModule.Data.Services
{
    public class TaggedItemOutlinesSynchronizator : ITaggedItemOutlinesSynchronizator
    {
        private readonly ITaggedItemService _taggedItemService;
        private readonly ITaggedEntitiesServiceFactory _taggedEntriesServiceFactory;
        public TaggedItemOutlinesSynchronizator(ITaggedItemService tagItemService, ITaggedEntitiesServiceFactory taggedEntriesServiceFactory)
        {
            _taggedItemService = tagItemService;
            _taggedEntriesServiceFactory = taggedEntriesServiceFactory;
        }

        public void SynchronizeOutlines(TaggedItem[] taggedItems)
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

                var taggedEntities = entitiesService.GetEntitiesByIds(group.Select(x => x.EntityId).Distinct().ToArray());
                foreach (var taggedItem in group)
                {
                    var referencedEntity = taggedEntities.FirstOrDefault(x => x.Id.EqualsInvariant(taggedItem.EntityId));
                    if (referencedEntity == null)
                    {
                        toRemoveTaggedItemIds.Add(taggedItem.Id);
                    }
                    else if (referencedEntity is IHasOutlines hasOutlines)
                    {
                        taggedItem.Outlines = hasOutlines.Outlines.Select(x => x.ToString()).Select(x => new Core.Model.TaggedItemOutline { Outline = x }).ToList();
                    }
                }

                _taggedItemService.SaveTaggedItems(group.ToArray());

                if (toRemoveTaggedItemIds.Any())
                {
                    _taggedItemService.DeleteTaggedItems(toRemoveTaggedItemIds.ToArray());
                }
            }
        }
    }
}
