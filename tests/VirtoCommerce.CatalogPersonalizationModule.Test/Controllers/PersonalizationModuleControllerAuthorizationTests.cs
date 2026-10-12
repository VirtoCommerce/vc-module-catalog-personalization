using System.Linq;
using System.Reflection;
using Microsoft.AspNetCore.Authorization;
using VirtoCommerce.CatalogPersonalizationModule.Core;
using VirtoCommerce.CatalogPersonalizationModule.Web.Authorization;
using VirtoCommerce.CatalogPersonalizationModule.Web.Controllers.Api;
using Xunit;

namespace VirtoCommerce.CatalogPersonalizationModule.Test.Controllers
{
    public class PersonalizationModuleControllerAuthorizationTests
    {
        [Fact]
        public void Controller_Requires_Authenticated_User()
        {
            Assert.NotNull(typeof(PersonalizationModuleController).GetCustomAttribute<AuthorizeAttribute>());
        }

        [Theory]
        [InlineData(nameof(PersonalizationModuleController.GetTaggedItem), CatalogReadAuthorizationRequirement.PolicyName)]
        [InlineData(nameof(PersonalizationModuleController.GetTagsCount), CatalogReadAuthorizationRequirement.PolicyName)]
        [InlineData(nameof(PersonalizationModuleController.Search), CatalogReadAuthorizationRequirement.PolicyName)]
        [InlineData(nameof(PersonalizationModuleController.UpdateTaggedItem), ModuleConstants.Security.Permissions.Update)]
        [InlineData(nameof(PersonalizationModuleController.RunOutlinesSynchronization), ModuleConstants.Security.Permissions.Update)]
        [InlineData(nameof(PersonalizationModuleController.CancelSynchronization), ModuleConstants.Security.Permissions.Update)]
        public void Action_Requires_Permission(string actionName, string permission)
        {
            var policies = typeof(PersonalizationModuleController).GetMethod(actionName)
                .GetCustomAttributes<AuthorizeAttribute>()
                .Select(x => x.Policy);

            Assert.Contains(permission, policies);
        }

        [Fact]
        public void Every_Action_Requires_A_Permission()
        {
            var actions = typeof(PersonalizationModuleController).GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly);

            Assert.NotEmpty(actions);
            Assert.All(actions, action =>
            {
                Assert.Null(action.GetCustomAttribute<AllowAnonymousAttribute>());
                Assert.Contains(action.GetCustomAttributes<AuthorizeAttribute>(), x => !string.IsNullOrEmpty(x.Policy));
            });
        }
    }
}
