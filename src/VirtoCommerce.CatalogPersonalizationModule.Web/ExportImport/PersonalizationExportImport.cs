using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;
using VirtoCommerce.CatalogPersonalizationModule.Core.Model;
using VirtoCommerce.CatalogPersonalizationModule.Core.Model.Search;
using VirtoCommerce.CatalogPersonalizationModule.Core.Services;
using VirtoCommerce.Platform.Core.Common;
using VirtoCommerce.Platform.Core.ExportImport;
using VirtoCommerce.Platform.Data.ExportImport;

namespace VirtoCommerce.CatalogPersonalizationModule.Web.ExportImport
{
    public sealed class PersonalizationExportImport
    {
        private readonly ITaggedItemService _taggedItemService;
        private readonly ITaggedItemSearchService _taggedItemSearchService;
        private readonly JsonSerializer _serializer;
        private readonly int _batchSize = 50;

        public PersonalizationExportImport(ITaggedItemService taggedItemService, ITaggedItemSearchService taggedItemSearchService, JsonSerializer jsonSerializer)
        {
            _taggedItemService = taggedItemService;
            _taggedItemSearchService = taggedItemSearchService;
            _serializer = jsonSerializer;
        }

        public async Task DoExportAsync(Stream outStream, Action<ExportImportProgressInfo> progressCallback, ICancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var progressInfo = new ExportImportProgressInfo {Description = "loading data..."};
            progressCallback(progressInfo);

            using (var sw = new StreamWriter(outStream, System.Text.Encoding.UTF8))
            using (var writer = new JsonTextWriter(sw))
            {
                await writer.WriteStartObjectAsync();

                var taggedItems = await _taggedItemSearchService.SearchTaggedItemsAsync(new TaggedItemSearchCriteria {Take = int.MaxValue});
                progressInfo.Description = $"{taggedItems.TotalCount} tagged items loading...";
                progressCallback(progressInfo);

                await writer.WritePropertyNameAsync("TaggedItemsTotalCount");
                await writer.WriteValueAsync(taggedItems.TotalCount);

                await writer.WritePropertyNameAsync("TaggedItems");
                await writer.WriteStartArrayAsync();

                foreach (var taggedItem in taggedItems.Results)
                {
                    _serializer.Serialize(writer, taggedItem);
                }

                await writer.FlushAsync();
                progressInfo.Description = $"{taggedItems.TotalCount} tagged items exported";
                progressCallback(progressInfo);

                await writer.WriteEndArrayAsync();
                await writer.WriteEndObjectAsync();
                await writer.FlushAsync();
            }
        }

        public async Task DoImportAsync(Stream inputStream, Action<ExportImportProgressInfo> progressCallback, ICancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var progressInfo = new ExportImportProgressInfo();
            progressInfo.Description = "tagged items importing...";
            progressCallback(progressInfo);

            using (var streamReader = new StreamReader(inputStream))
            using (var reader = new JsonTextReader(streamReader))
            {
                while (reader.Read())
                {
                    if (reader.TokenType == JsonToken.PropertyName)
                    {
                        if (reader.Value.ToString() == "TaggedItems")
                        {
                            await reader.DeserializeJsonArrayWithPagingAsync<TaggedItem>(_serializer, _batchSize,
                                async items => { await _taggedItemService.SaveChangesAsync(items.ToArray()); }, processedCount =>
                                {
                                    progressInfo.Description = $"{processedCount} Tagged items have been imported";
                                    progressCallback(progressInfo);
                                }, cancellationToken);
                        }
                    }
                }
            }
        }
    }
}
