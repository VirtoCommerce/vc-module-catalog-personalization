using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using VirtoCommerce.CatalogPersonalizationModule.Core;
using VirtoCommerce.CatalogPersonalizationModule.Core.Events;
using VirtoCommerce.CatalogPersonalizationModule.Data.Search.Indexing;
using VirtoCommerce.Platform.Core.Common;
using VirtoCommerce.Platform.Core.Events;
using VirtoCommerce.Platform.Core.Jobs;
using VirtoCommerce.Platform.Core.Settings;
using VirtoCommerce.SearchModule.Core.BackgroundJobs;
using VirtoCommerce.SearchModule.Core.Extensions;
using VirtoCommerce.SearchModule.Core.Model;

namespace VirtoCommerce.CatalogPersonalizationModule.Data.Handlers;

public class TaggedItemChangedEventHandler : IEventHandler<TaggedItemChangedEvent>
{
    private readonly ISettingsManager _settingsManager;
    private readonly IIndexingJobService _indexingJobService;
    private readonly IEnumerable<IndexDocumentConfiguration> _configurations;

    public TaggedItemChangedEventHandler(
        ISettingsManager settingsManager,
        IIndexingJobService indexingJobService,
        IEnumerable<IndexDocumentConfiguration> configurations)
    {
        _settingsManager = settingsManager;
        _indexingJobService = indexingJobService;
        _configurations = configurations;
    }

    public async Task Handle(TaggedItemChangedEvent message)
    {
        if (await _settingsManager.GetValueAsync<bool>(ModuleConstants.Settings.General.EventBasedIndexation))
        {
            if (message == null)
            {
                throw new ArgumentNullException(nameof(message));
            }

            var productIndexChanges = message.ChangedEntries
                .Where(x => x.OldEntry.EntityType.EqualsIgnoreCase(KnownDocumentTypes.Product))
                .Select(x => new IndexEntry
                {
                    Id = x.OldEntry.EntityId,
                    Type = x.OldEntry.EntityType,
                    EntryState = EntryState.Modified,
                })
                .ToArray();

            _indexingJobService.EnqueueIndexAndDeleteDocuments(productIndexChanges, JobPriority.Normal,
                _configurations.GetDocumentBuilders(KnownDocumentTypes.Product, typeof(ProductTaggedItemDocumentBuilder)).ToList());

            var categoryIndexChanges = message.ChangedEntries
                .Where(x => x.OldEntry.EntityType.EqualsIgnoreCase(KnownDocumentTypes.Category))
                .Select(x => new IndexEntry
                {
                    Id = x.OldEntry.EntityId,
                    Type = x.OldEntry.EntityType,
                    EntryState = EntryState.Modified,
                })
                .ToArray();

            _indexingJobService.EnqueueIndexAndDeleteDocuments(categoryIndexChanges, JobPriority.Normal,
                _configurations.GetDocumentBuilders(KnownDocumentTypes.Category, typeof(CategoryTaggedItemDocumentBuilder)).ToList());
        }
    }
}
