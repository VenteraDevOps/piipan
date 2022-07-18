using Microsoft.Extensions.Options;
using Piipan.Shared.Roles;
using Xunit;

namespace Piipan.Shared.Tests.Roles
{
    public class RolesProviderTests
    {
        RoleOptions roleOptions = new RoleOptions
        {
            EditMatch = new string[] { "Worker" }
        };

        /// <summary>
        /// Get the
        /// </summary>
        [Fact]
        public void GetEditMatchRoles()
        {
            // Arrange
            var options = Options.Create(roleOptions);
            var rolesProvider = new RolesProvider(options);

            // Act
            var roles = rolesProvider.GetMatchEditRoles();

            // Assert
            Assert.Equal(new string[] { "Worker" }, roles);
        }
    }
}