using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.DependencyInjection;
using VirtoCommerce.CatalogPersonalizationModule.Web.Authorization;
using VirtoCommerce.Platform.Core;
using Xunit;

namespace VirtoCommerce.CatalogPersonalizationModule.Test.Controllers
{
    public class CatalogReadAuthorizationTests
    {
        private const string PermissionClaimType = PlatformConstants.Security.Claims.PermissionClaimType;

        [Theory]
        [InlineData("catalog:read")]
        [InlineData("catalog:categories:read")]
        [InlineData("catalog:products:read")]
        [InlineData("catalog:read|[{\"Type\":\"SelectedCatalogScope\",\"Scope\":\"catalog-1\"}]")]
        [InlineData("catalog:categories:read|[{\"Type\":\"SelectedCatalogScope\",\"Scope\":\"catalog-1\"}]")]
        public async Task Succeeds_For_Catalog_Read_Permission(string permissionClaim)
        {
            var result = await AuthorizeAsync(Authenticated(new Claim(PermissionClaimType, permissionClaim)));

            Assert.True(result.Succeeded);
        }

        [Fact]
        public async Task Succeeds_For_Administrator()
        {
            var result = await AuthorizeAsync(Authenticated(new Claim(ClaimTypes.Role, PlatformConstants.Security.SystemRoles.Administrator)));

            Assert.True(result.Succeeded);
        }

        [Fact]
        public async Task Fails_For_Anonymous_User()
        {
            var user = new ClaimsPrincipal(new ClaimsIdentity([new Claim(PermissionClaimType, "catalog:read")]));

            var result = await AuthorizeAsync(user);

            Assert.False(result.Succeeded);
        }

        [Theory]
        [InlineData("catalog:access")]
        [InlineData("catalog:update")]
        [InlineData("catalog:products:update")]
        [InlineData("catalog:readonly")]
        public async Task Fails_Without_Catalog_Read_Permission(string permissionClaim)
        {
            var result = await AuthorizeAsync(Authenticated(new Claim(PermissionClaimType, permissionClaim)));

            Assert.False(result.Succeeded);
        }

        [Fact]
        public async Task Limited_Permissions_Claim_Restricts_Access()
        {
            var user = Authenticated(
                new Claim(PermissionClaimType, "catalog:read"),
                new Claim(PlatformConstants.Security.Claims.LimitedPermissionsClaimType, "order:read;customer:read"));

            var result = await AuthorizeAsync(user);

            Assert.False(result.Succeeded);
        }

        [Fact]
        public async Task Limited_Permissions_Claim_With_Catalog_Read_Succeeds()
        {
            var user = Authenticated(new Claim(PlatformConstants.Security.Claims.LimitedPermissionsClaimType, "order:read;catalog:products:read"));

            var result = await AuthorizeAsync(user);

            Assert.True(result.Succeeded);
        }

        private static Task<AuthorizationResult> AuthorizeAsync(ClaimsPrincipal user)
        {
            var services = new ServiceCollection();
            services.AddLogging();
            services.AddCatalogReadAuthorization();
            var authorizationService = services.BuildServiceProvider().GetRequiredService<IAuthorizationService>();

            return authorizationService.AuthorizeAsync(user, CatalogReadAuthorizationRequirement.PolicyName);
        }

        private static ClaimsPrincipal Authenticated(params Claim[] claims)
        {
            return new ClaimsPrincipal(new ClaimsIdentity(claims, authenticationType: "Test"));
        }
    }
}
