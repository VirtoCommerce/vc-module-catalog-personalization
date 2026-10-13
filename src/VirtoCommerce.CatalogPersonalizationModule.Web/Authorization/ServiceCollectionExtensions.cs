using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.DependencyInjection;
using VirtoCommerce.Platform.Core;

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
                    .AddAuthenticationSchemes(PlatformConstants.Security.AuthenticationSchemes.MixedScheme)
                    .AddRequirements(new CatalogReadAuthorizationRequirement()));
            });

            return services;
        }
    }
}
