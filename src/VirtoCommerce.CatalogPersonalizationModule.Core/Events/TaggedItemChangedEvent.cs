using System.Collections.Generic;
using VirtoCommerce.CatalogPersonalizationModule.Core.Model;
using VirtoCommerce.Platform.Core.Events;

namespace VirtoCommerce.CatalogPersonalizationModule.Core.Events;

public class TaggedItemChangedEvent : GenericChangedEntryEvent<TaggedItem>
{
    public TaggedItemChangedEvent(IEnumerable<GenericChangedEntry<TaggedItem>> changedEntries)
        : base(changedEntries)
    {
    }
}
