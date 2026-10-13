using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.DependencyInjection;

namespace VirtoCommerce.CatalogPersonalizationModule.Web.Authorization
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddCatalogReadAuthorization(this IServiceCollection services)
        {
            services.AddSingleton<IAuthorizationHandler, CatalogReadAuthorizationHandler>();
            services.AddAuthorizationCore(options =>
            {
                options.AddPolicy(CatalogReadAuthorizationRequirement.PolicyName, policy => policy
                    .AddAuthenticationSchemes("VirtoMixedAuthenticationScheme")
                    .AddRequirements(new CatalogReadAuthorizationRequirement()));
            });

            return services;
        }
    }
}
