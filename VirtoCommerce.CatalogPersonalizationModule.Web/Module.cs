using Microsoft.Practices.Unity;
using VirtoCommerce.Platform.Core.Modularity;

namespace VirtoCommerce.CatalogPersonalizationModule.Web
{
    public class Module : ModuleBase
    {
        // private const string _connectionStringName = "VirtoCommerce";
        private readonly IUnityContainer _container;

        public Module(IUnityContainer container)
        {
            _container = container;
        }

        public override void SetupDatabase()
        {
            // Modify database schema with EF migrations
            // using (var context = new PricingRepositoryImpl(_connectionStringName))
            // {
            //     var initializer = new SetupDatabaseInitializer<MyRepository, Data.Migrations.Configuration>();
            //     initializer.InitializeDatabase(context);
            // }
        }

        public override void Initialize()
        {
            base.Initialize();

            // This method is called for each installed module on the first stage of initialization.

            // Register implementations:
            // _container.RegisterType<IMyRepository>(new InjectionFactory(c => new MyRepository(_connectionStringName, new EntityPrimaryKeyGeneratorInterceptor()));
            // _container.RegisterType<IMyService, MyServiceImplementation>();

            // Try to avoid calling _container.Resolve<>();
        }

        public override void PostInitialize()
        {
            base.PostInitialize();

            // This method is called for each installed module on the second stage of initialization.

            // Register implementations 
            // _container.RegisterType<IMyService, MyService>();

            // Resolve registered implementations:
            // var settingManager = _container.Resolve<ISettingsManager>();
            // var value = settingManager.GetValue("Pricing.ExportImport.Description", string.Empty);
        }
    }
}
