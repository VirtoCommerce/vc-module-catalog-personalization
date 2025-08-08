using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Hangfire;
using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using VirtoCommerce.CatalogModule.Core.Search;
using VirtoCommerce.CatalogPersonalizationModule.Core;
using VirtoCommerce.CatalogPersonalizationModule.Core.Events;
using VirtoCommerce.CatalogPersonalizationModule.Core.Services;
using VirtoCommerce.CatalogPersonalizationModule.Data.Handlers;
using VirtoCommerce.CatalogPersonalizationModule.Data.MySql;
using VirtoCommerce.CatalogPersonalizationModule.Data.PostgreSql;
using VirtoCommerce.CatalogPersonalizationModule.Data.Repositories;
using VirtoCommerce.CatalogPersonalizationModule.Data.Search;
using VirtoCommerce.CatalogPersonalizationModule.Data.Search.Indexing;
using VirtoCommerce.CatalogPersonalizationModule.Data.Services;
using VirtoCommerce.CatalogPersonalizationModule.Data.SqlServer;
using VirtoCommerce.CatalogPersonalizationModule.Web.BackgroundJobs;
using VirtoCommerce.CatalogPersonalizationModule.Web.ExportImport;
using VirtoCommerce.Platform.Core.Common;
using VirtoCommerce.Platform.Core.Events;
using VirtoCommerce.Platform.Core.ExportImport;
using VirtoCommerce.Platform.Core.Modularity;
using VirtoCommerce.Platform.Core.Security;
using VirtoCommerce.Platform.Core.Settings;
using VirtoCommerce.Platform.Data.Extensions;
using VirtoCommerce.Platform.Hangfire;
using VirtoCommerce.Platform.Hangfire.Extensions;
using VirtoCommerce.SearchModule.Core.Model;
using VirtoCommerce.SearchModule.Core.Services;

namespace VirtoCommerce.CatalogPersonalizationModule.Web
{
    public class Module : IModule, IExportSupport, IImportSupport, IHasConfiguration
    {
        private IApplicationBuilder _appBuilder;

        public ManifestModuleInfo ModuleInfo { get; set; }
        public IConfiguration Configuration { get; set; }

        public void Initialize(IServiceCollection serviceCollection)
        {
            serviceCollection.AddDbContext<PersonalizationDbContext>(options =>
            {
                var databaseProvider = Configuration.GetValue("DatabaseProvider", "SqlServer");
                var connectionString = Configuration.GetConnectionString(ModuleInfo.Id) ?? Configuration.GetConnectionString("VirtoCommerce");

                switch (databaseProvider)
                {
                    case "MySql":
                        options.UseMySqlDatabase(connectionString);
                        break;
                    case "PostgreSql":
                        options.UsePostgreSqlDatabase(connectionString);
                        break;
                    default:
                        options.UseSqlServerDatabase(connectionString);
                        break;
                }
            });

            serviceCollection.AddTransient<Func<IPersonalizationRepository>>(provider => () => provider.CreateScope().ServiceProvider.GetRequiredService<IPersonalizationRepository>());

            serviceCollection.AddTransient<IPersonalizationRepository, PersonalizationRepository>();
            serviceCollection.AddTransient<ITaggedItemService, PersonalizationService>();
            serviceCollection.AddTransient<ITaggedItemSearchService, PersonalizationService>();
            serviceCollection.AddTransient<ITaggedEntitiesServiceFactory, TaggedEntitiesServiceFactory>();
            serviceCollection.AddTransient<ITaggedItemOutlinesSynchronizer, TaggedItemOutlinesSynchronizer>();

            serviceCollection.AddTransient<ProductSearchUserGroupsRequestBuilder>();
            serviceCollection.AddTransient<CategorySearchUserGroupsRequestBuilder>();

            serviceCollection.AddSingleton<TaggedItemIndexChangesProvider>();
            serviceCollection.AddSingleton<ProductTaggedItemDocumentBuilder>();
            serviceCollection.AddSingleton<CategoryTaggedItemDocumentBuilder>();

            serviceCollection.AddSingleton<PersonalizationExportImport>();

            serviceCollection.AddTransient<LogChangesChangedEventHandler>();
            serviceCollection.AddTransient<TaggedItemChangedEventHandler>();

            serviceCollection.AddTransient<ITagPropagationPolicy>(provider =>
            {
                var settingsManager = provider.GetService<ISettingsManager>();
                var repositoryFactory = provider.GetService<Func<IPersonalizationRepository>>();
                var listEntrySearchService = provider.GetService<IListEntrySearchService>();

                var tagsInheritancePolicy = settingsManager.GetValue<string>(ModuleConstants.Settings.General.TagsInheritancePolicy);
                if (tagsInheritancePolicy.EqualsInvariant("DownTree"))
                {
                    return new DownTreeTagPropagationPolicy(repositoryFactory);
                }
                else
                {
                    return new UpTreeTagPropagationPolicy(repositoryFactory, listEntrySearchService);
                }
            });
        }

        public void PostInitialize(IApplicationBuilder appBuilder)
        {
            _appBuilder = appBuilder;

            var settingsRegistrar = appBuilder.ApplicationServices.GetRequiredService<ISettingsRegistrar>();
            settingsRegistrar.RegisterSettings(ModuleConstants.Settings.General.AllSettings, ModuleInfo.Id);

            var permissionsRegistrar = appBuilder.ApplicationServices.GetRequiredService<IPermissionsRegistrar>();
            permissionsRegistrar.RegisterPermissions(ModuleInfo.Id, "Catalog Personalization", ModuleConstants.Security.Permissions.AllPermissions);

            var recurringJobManager = appBuilder.ApplicationServices.GetService<IRecurringJobManager>();
            var settingsManager = appBuilder.ApplicationServices.GetService<ISettingsManager>();

            recurringJobManager.WatchJobSetting(
                settingsManager,
                new SettingCronJobBuilder()
                    .SetEnabledEvaluator(x => "UpTree".EqualsInvariant((string)x))
                    .SetEnablerSetting(ModuleConstants.Settings.General.TagsInheritancePolicy)
                    .SetCronSetting(ModuleConstants.Settings.General.CronExpression)
                    .ToJob<TaggedItemOutlinesSynchronizationJob>(x => x.Run())
                    .Build());

            // Add tagged items document source to the category or product indexing configuration
            var documentIndexingConfigurations = appBuilder.ApplicationServices.GetRequiredService<IEnumerable<IndexDocumentConfiguration>>()?.ToList();
            if (documentIndexingConfigurations != null)
            {
                //Category indexing
                var taggedItemCategoryDocumentSource = new IndexDocumentSource
                {
                    ChangesProvider = appBuilder.ApplicationServices.GetRequiredService<TaggedItemIndexChangesProvider>(),
                    DocumentBuilder = appBuilder.ApplicationServices.GetRequiredService<CategoryTaggedItemDocumentBuilder>()
                };
                foreach (var configuration in documentIndexingConfigurations.Where(c => c.DocumentType == KnownDocumentTypes.Category))
                {
                    configuration.RelatedSources ??= new List<IndexDocumentSource>();
                    configuration.RelatedSources.Add(taggedItemCategoryDocumentSource);
                }

                //Product indexing
                var taggedItemProductDocumentSource = new IndexDocumentSource
                {
                    ChangesProvider = appBuilder.ApplicationServices.GetRequiredService<TaggedItemIndexChangesProvider>(),
                    DocumentBuilder = appBuilder.ApplicationServices.GetRequiredService<ProductTaggedItemDocumentBuilder>()
                };
                foreach (var configuration in documentIndexingConfigurations.Where(c => c.DocumentType == KnownDocumentTypes.Product))
                {
                    configuration.RelatedSources ??= new List<IndexDocumentSource>();
                    configuration.RelatedSources.Add(taggedItemProductDocumentSource);
                }
            }

            using (var serviceScope = appBuilder.ApplicationServices.CreateScope())
            {
                var databaseProvider = Configuration.GetValue("DatabaseProvider", "SqlServer");
                var dbContext = serviceScope.ServiceProvider.GetRequiredService<PersonalizationDbContext>();
                if (databaseProvider == "SqlServer")
                {
                    dbContext.Database.MigrateIfNotApplied(MigrationName.GetUpdateV2MigrationName(ModuleInfo.Id));
                }
                dbContext.Database.Migrate();
            }

            var searchRequestBuilderRegistrar = appBuilder.ApplicationServices.GetService<ISearchRequestBuilderRegistrar>();

            searchRequestBuilderRegistrar.Override(KnownDocumentTypes.Product, appBuilder.ApplicationServices.GetService<ProductSearchUserGroupsRequestBuilder>);
            searchRequestBuilderRegistrar.Override(KnownDocumentTypes.Category, appBuilder.ApplicationServices.GetService<CategorySearchUserGroupsRequestBuilder>);

            appBuilder.RegisterEventHandler<TaggedItemChangedEvent, LogChangesChangedEventHandler>();
            appBuilder.RegisterEventHandler<TaggedItemChangedEvent, TaggedItemChangedEventHandler>();
        }

        public void Uninstall()
        {
            throw new NotImplementedException();
        }

        public async Task ExportAsync(Stream outStream, ExportImportOptions options, Action<ExportImportProgressInfo> progressCallback, ICancellationToken cancellationToken)
        {
            await _appBuilder.ApplicationServices.GetRequiredService<PersonalizationExportImport>().DoExportAsync(outStream, progressCallback, cancellationToken);
        }

        public async Task ImportAsync(Stream inputStream, ExportImportOptions options, Action<ExportImportProgressInfo> progressCallback, ICancellationToken cancellationToken)
        {
            await _appBuilder.ApplicationServices.GetRequiredService<PersonalizationExportImport>().DoImportAsync(inputStream, progressCallback, cancellationToken);
        }
    }
}
