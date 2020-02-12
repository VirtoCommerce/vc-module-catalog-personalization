using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using VirtoCommerce.CatalogPersonalizationModule.Core.Model;
using VirtoCommerce.CatalogPersonalizationModule.Core.Model.Search;
using VirtoCommerce.CatalogPersonalizationModule.Core.Services;
using VirtoCommerce.Platform.Core.Common;
using VirtoCommerce.Platform.Core.ExportImport;

namespace VirtoCommerce.CatalogPersonalizationModule.Web.ExportImport
{
    public sealed class BackupObject
    {
        public ICollection<TaggedItem> TaggedItems { get; set; }
    }

    public sealed class PersonalizationExportImport
    {
        private readonly ITaggedItemService _taggedItemService;
        private readonly ITaggedItemSearchService _taggedItemSearchService;

        public PersonalizationExportImport(ITaggedItemService taggedItemService, ITaggedItemSearchService taggedItemSearchService)
        {
            _taggedItemService = taggedItemService;
            _taggedItemSearchService = taggedItemSearchService;
        }

        public async Task DoExportAsync(Stream backupStream, Action<ExportImportProgressInfo> progressCallback)
        {
            var backupObject = await GetBackupObjectAsync(progressCallback);
            backupObject.SerializeJson(backupStream);
        }

        public async Task DoImportAsync(Stream backupStream, Action<ExportImportProgressInfo> progressCallback)
        {
            var backupObject = backupStream.DeserializeJson<BackupObject>();
            var progressInfo = new ExportImportProgressInfo();

            progressInfo.Description = String.Format("{0} tagged items importing...", backupObject.TaggedItems.Count);
            progressCallback(progressInfo);

            await _taggedItemService.SaveTaggedItemsAsync(backupObject.TaggedItems.ToArray());
        }

        private async Task<BackupObject> GetBackupObjectAsync(Action<ExportImportProgressInfo> progressCallback)
        {
            var taggedItems = await _taggedItemSearchService.SearchTaggedItemsAsync(new TaggedItemSearchCriteria { Take = int.MaxValue });
            var progressInfo = new ExportImportProgressInfo { Description = String.Format("{0} tagged items loading...", taggedItems.TotalCount) };
            progressCallback(progressInfo);
            var retVal = new BackupObject
            {
                TaggedItems = taggedItems.Results
            };

            return retVal;
        }
    }
}
