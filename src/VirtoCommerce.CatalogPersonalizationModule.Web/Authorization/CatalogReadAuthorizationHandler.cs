using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using VirtoCommerce.Platform.Core;
using CatalogPermissions = VirtoCommerce.CatalogModule.Core.ModuleConstants.Security.Permissions;

namespace VirtoCommerce.CatalogPersonalizationModule.Web.Authorization
{
    /// <summary>
    /// Succeeds when the user can read catalog entities: <c>catalog:read</c> or one of the granular
    /// <c>catalog:categories:read</c> / <c>catalog:products:read</c> permissions, assigned globally or scoped to catalogs.
    /// </summary>
    public sealed class CatalogReadAuthorizationHandler : AuthorizationHandler<CatalogReadAuthorizationRequirement>
    {
        // Scoped permissions are stored as "<permission>|<serialized scopes>" claims
        private const char ScopeSeparator = '|';

        private static readonly string[] _readPermissions =
        [
            CatalogPermissions.Read
        ];

        protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, CatalogReadAuthorizationRequirement requirement)
        {
            if (HasReadPermission(context.User))
            {
                context.Succeed(requirement);
            }

            return Task.CompletedTask;
        }

        private static bool HasReadPermission(ClaimsPrincipal user)
        {
            if (user?.Identity?.IsAuthenticated != true)
            {
                return false;
            }

            // Same rule as the platform permission handler: when present, the limited permissions claim is the only source of permissions
            var limitedPermissionsClaim = user.FindFirst(PlatformConstants.Security.Claims.LimitedPermissionsClaimType)?.Value;
            if (limitedPermissionsClaim != null)
            {
                var limitedPermissions = limitedPermissionsClaim.Split(PlatformConstants.Security.Claims.PermissionClaimTypeDelimiter, StringSplitOptions.RemoveEmptyEntries);

                return _readPermissions.Any(limitedPermissions.Contains);
            }

            if (user.IsInRole(PlatformConstants.Security.SystemRoles.Administrator))
            {
                return true;
            }

            return user
                .FindAll(PlatformConstants.Security.Claims.PermissionClaimType)
                .Select(x => x.Value.Split(ScopeSeparator)[0])
                .Any(_readPermissions.Contains);
        }
    }
}
