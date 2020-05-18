using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using VirtoCommerce.CatalogModule.Core.Search;
using VirtoCommerce.CatalogPersonalizationModule.Core;
using VirtoCommerce.CatalogPersonalizationModule.Core.Services;
using VirtoCommerce.CatalogPersonalizationModule.Data.Common;
using VirtoCommerce.CatalogPersonalizationModule.Data.Handlers;
using VirtoCommerce.CatalogPersonalizationModule.Data.Repositories;
using VirtoCommerce.CatalogPersonalizationModule.Data.Search;
using VirtoCommerce.CatalogPersonalizationModule.Data.Search.Indexing;
using VirtoCommerce.CatalogPersonalizationModule.Data.Services;
using VirtoCommerce.CatalogPersonalizationModule.Web.ExportImport;
using VirtoCommerce.Platform.Core.Bus;
using VirtoCommerce.Platform.Core.Common;
using VirtoCommerce.Platform.Core.ExportImport;
using VirtoCommerce.Platform.Core.Modularity;
using VirtoCommerce.Platform.Core.Security;
using VirtoCommerce.Platform.Core.Settings;
using VirtoCommerce.Platform.Core.Settings.Events;
using VirtoCommerce.Platform.Data.Extensions;
using VirtoCommerce.SearchModule.Core.Model;
using VirtoCommerce.SearchModule.Core.Services;

namespace VirtoCommerce.CatalogPersonalizationModule.Web
{
    public class Module : IModule, IExportSupport, IImportSupport
    {
        private IApplicationBuilder _appBuilder;
        public ManifestModuleInfo ModuleInfo { get; set; }

        public void Initialize(IServiceCollection serviceCollection)
        {
            var configuration = serviceCollection.BuildServiceProvider().GetRequiredService<IConfiguration>();
            serviceCollection.AddTransient<IPersonalizationRepository, PersonalizationRepository>();
            var connectionString = configuration.GetConnectionString("VirtoCommerce");
            serviceCollection.AddDbContext<PersonalizationDbContext>(options => options.UseSqlServer(connectionString));
            serviceCollection.AddTransient<Func<IPersonalizationRepository>>(provider => () => provider.CreateScope().ServiceProvider.GetRequiredService<IPersonalizationRepository>());

            serviceCollection.AddTransient<ITaggedItemService, PersonalizationService>();
            serviceCollection.AddTransient<ITaggedItemSearchService, PersonalizationService>();
            serviceCollection.AddTransient<ITaggedEntitiesServiceFactory, TaggedEntitiesServiceFactory>();
            serviceCollection.AddTransient<ITaggedItemOutlinesSynchronizer, TaggedItemOutlinesSynchronizer>();

            serviceCollection.AddTransient<ProductSearchUserGroupsRequestBuilder>();
            serviceCollection.AddTransient<CategorySearchUserGroupsRequestBuilder>();

            serviceCollection.AddTransient<TaggedItemIndexChangesProvider>();
            serviceCollection.AddTransient<ProductTaggedItemDocumentBuilder>();
            serviceCollection.AddTransient<CategoryTaggedItemDocumentBuilder>();

            serviceCollection.AddTransient<PersonalizationExportImport>();

            serviceCollection.AddTransient<ITagPropagationPolicy>(provider =>
            {
                var settingsManager = provider.GetService<ISettingsManager>();
                var repositoryFactory = provider.GetService<Func<IPersonalizationRepository>>();

                var tagsInheritancePolicy = settingsManager.GetValue(ModuleConstants.Settings.General.TagsInheritancePolicy.Name, "DownTree");
                if (tagsInheritancePolicy.EqualsInvariant("DownTree"))
                {
                    return new DownTreeTagPropagationPolicy(repositoryFactory);
                }
                else
                {
                    var listEntrySearchService = provider.GetService<IListEntrySearchService>();
                    return new UpTreeTagPropagationPolicy(repositoryFactory, listEntrySearchService);
                }
            });

            serviceCollection.AddTransient<ObjectSettingEntryChangedEventHandler>();
            serviceCollection.AddTransient<ModuleConfigurator>();
        }

        public void PostInitialize(IApplicationBuilder appBuilder)
        {

            _appBuilder = appBuilder;
            var settingsRegistrar = appBuilder.ApplicationServices.GetRequiredService<ISettingsRegistrar>();
            settingsRegistrar.RegisterSettings(ModuleConstants.Settings.General.AllSettings, ModuleInfo.Id);

            var permissionsProvider = appBuilder.ApplicationServices.GetRequiredService<IPermissionsRegistrar>();
            permissionsProvider.RegisterPermissions(ModuleConstants.Security.Permissions.AllPermissions.Select(x => new Permission { GroupName = "Catalog Personalization", Name = x }).ToArray());

            //Subscribe for Orders Synchronization job configuration changes
            var inProcessBus = appBuilder.ApplicationServices.GetService<IHandlerRegistrar>();
            inProcessBus.RegisterHandler<ObjectSettingChangedEvent>(async (message, token) => await appBuilder.ApplicationServices.GetService<ObjectSettingEntryChangedEventHandler>().Handle(message));

            //Schedule periodic OutlinesSynchronizationJob job
            var moduleConfigurator = appBuilder.ApplicationServices.GetService<ModuleConfigurator>();
            moduleConfigurator.ConfigureOutlinesSynchronizationJob().GetAwaiter().GetResult();

            moduleConfigurator.ConfigureSearch();

            using (var serviceScope = appBuilder.ApplicationServices.CreateScope())
            {
                var dbContext = serviceScope.ServiceProvider.GetRequiredService<PersonalizationDbContext>();
                dbContext.Database.MigrateIfNotApplied(MigrationName.GetUpdateV2MigrationName(ModuleInfo.Id));
                dbContext.Database.EnsureCreated();
                dbContext.Database.Migrate();
            }

            var searchRequestBuilderRegistrar = appBuilder.ApplicationServices.GetService<ISearchRequestBuilderRegistrar>();

            searchRequestBuilderRegistrar.Override(KnownDocumentTypes.Product, appBuilder.ApplicationServices.GetService<ProductSearchUserGroupsRequestBuilder>);
            searchRequestBuilderRegistrar.Override(KnownDocumentTypes.Category, appBuilder.ApplicationServices.GetService<CategorySearchUserGroupsRequestBuilder>);
        }

        public void Uninstall()
        {
            // Method intentionally left empty.
        }

        public async Task ExportAsync(Stream outStream, ExportImportOptions options, Action<ExportImportProgressInfo> progressCallback, ICancellationToken cancellationToken)
        {
            await _appBuilder.ApplicationServices.GetRequiredService<PersonalizationExportImport>().DoExportAsync(outStream, progressCallback);
        }

        public async Task ImportAsync(Stream inputStream, ExportImportOptions options, Action<ExportImportProgressInfo> progressCallback, ICancellationToken cancellationToken)
        {
            await _appBuilder.ApplicationServices.GetRequiredService<PersonalizationExportImport>().DoImportAsync(inputStream, progressCallback);
        }
    }
}
