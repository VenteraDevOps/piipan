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
        /// Make sure the RolesProvider is returning back the acceptable roles for editing a match
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

        /// <summary>
        /// Make sure the RolesProvider is returning back an empty list when roleOptions is null
        /// </summary>
        [Fact]
        public void GetEditMatchRolesWhenOptionsAreNull()
        {
            // Arrange
            var options = Options.Create<RoleOptions>(null);
            var rolesProvider = new RolesProvider(options);

            // Act
            var roles = rolesProvider.GetMatchEditRoles();

            // Assert
            Assert.Empty(roles);
        }

        /// <summary>
        /// Make sure the RolesProvider is returning back an empty list when roleOptions.EditMatch is null
        /// </summary>
        [Fact]
        public void GetEditMatchRolesWhenEditMatchIsNull()
        {
            // Arrange
            var options = Options.Create(new RoleOptions { EditMatch = null });
            var rolesProvider = new RolesProvider(options);

            // Act
            var roles = rolesProvider.GetMatchEditRoles();

            // Assert
            Assert.Empty(roles);
        }
    }
}