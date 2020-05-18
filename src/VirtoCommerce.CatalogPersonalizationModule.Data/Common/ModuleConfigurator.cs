using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Hangfire;
using VirtoCommerce.CatalogPersonalizationModule.Data.BackgroundJobs;
using VirtoCommerce.CatalogPersonalizationModule.Data.Search.Indexing;
using VirtoCommerce.Platform.Core.Common;
using VirtoCommerce.Platform.Core.Settings;
using VirtoCommerce.SearchModule.Core.Model;
using static VirtoCommerce.CatalogPersonalizationModule.Core.ModuleConstants;

namespace VirtoCommerce.CatalogPersonalizationModule.Data.Common
{
    public class ModuleConfigurator
    {
        private readonly ISettingsManager _settingsManager;
        private readonly IEnumerable<IndexDocumentConfiguration> _documentIndexingConfigurations;
        private readonly TaggedItemIndexChangesProvider _changesProvider;
        private readonly CategoryTaggedItemDocumentBuilder _categoryDocumentBuilder;
        private readonly ProductTaggedItemDocumentBuilder _productDocumentBuilder;

        public ModuleConfigurator(ISettingsManager settingsManager, IEnumerable<IndexDocumentConfiguration> documentIndexingConfigurations, TaggedItemIndexChangesProvider changesProvider, CategoryTaggedItemDocumentBuilder categoryDocumentBuilder, ProductTaggedItemDocumentBuilder productDocumentBuilder)
        {
            _settingsManager = settingsManager;
            _documentIndexingConfigurations = documentIndexingConfigurations;
            _changesProvider = changesProvider;
            _categoryDocumentBuilder = categoryDocumentBuilder;
            _productDocumentBuilder = productDocumentBuilder;
        }

        public async Task ConfigureOutlinesSynchronizationJob()
        {
            var tagsInheritancePolicy = await _settingsManager.GetValueAsync(Settings.General.TagsInheritancePolicy.Name, "DownTree");
            if (tagsInheritancePolicy.EqualsInvariant("UpTree"))
            {
                var cronExpression = _settingsManager.GetValue(Settings.General.CronExpression.Name, "0/15 * * * *");
                RecurringJob.AddOrUpdate<TaggedItemOutlinesSynchronizationJob>(TaggedItemOutlinesSynchronizationJob.JobId, x => x.Run(), cronExpression);
            }
            else
            {
                RecurringJob.RemoveIfExists(TaggedItemOutlinesSynchronizationJob.JobId);
            }
        }

        public void ConfigureSearch()
        {
            // Add tagged items document source to the category or product indexing configuration
            if (_documentIndexingConfigurations != null)
            {
                //Category indexing
                var taggedItemCategoryDocumentSource = new IndexDocumentSource
                {
                    ChangesProvider = _changesProvider,
                    DocumentBuilder = _categoryDocumentBuilder
                };
                foreach (var configuration in _documentIndexingConfigurations.Where(c => c.DocumentType == KnownDocumentTypes.Category))
                {
                    if (configuration.RelatedSources == null)
                    {
                        configuration.RelatedSources = new List<IndexDocumentSource>();
                    }

                    var oldSource = configuration.RelatedSources.FirstOrDefault(x => x.ChangesProvider.GetType() == _changesProvider.GetType() && x.DocumentBuilder.GetType() == _categoryDocumentBuilder.GetType());
                    if (oldSource != null)
                    {
                        configuration.RelatedSources.Remove(oldSource);
                    }

                    configuration.RelatedSources.Add(taggedItemCategoryDocumentSource);
                }

                //Product indexing
                var taggedItemProductDocumentSource = new IndexDocumentSource
                {
                    ChangesProvider = _changesProvider,
                    DocumentBuilder = _productDocumentBuilder
                };
                foreach (var configuration in _documentIndexingConfigurations.Where(c => c.DocumentType == KnownDocumentTypes.Product))
                {
                    if (configuration.RelatedSources == null)
                    {
                        configuration.RelatedSources = new List<IndexDocumentSource>();
                    }

                    var oldSource = configuration.RelatedSources.FirstOrDefault(x => x.ChangesProvider.GetType() == _changesProvider.GetType() && x.DocumentBuilder.GetType() == _productDocumentBuilder.GetType());
                    if (oldSource != null)
                    {
                        configuration.RelatedSources.Remove(oldSource);
                    }

                    configuration.RelatedSources.Add(taggedItemProductDocumentSource);
                }
            }
        }
    }
}
