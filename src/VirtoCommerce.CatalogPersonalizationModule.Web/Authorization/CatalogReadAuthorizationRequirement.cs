using Microsoft.AspNetCore.Authorization;

namespace VirtoCommerce.CatalogPersonalizationModule.Web.Authorization
{
    public sealed class CatalogReadAuthorizationRequirement : IAuthorizationRequirement
    {
        public const string PolicyName = "CatalogPersonalization:CatalogRead";
    }
}
