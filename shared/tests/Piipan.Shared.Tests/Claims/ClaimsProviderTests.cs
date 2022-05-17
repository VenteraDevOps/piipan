using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using System.Collections.Generic;
using System.Security.Claims;
using Xunit;

namespace Piipan.Shared.Claims.Tests
{
    public class ClaimsProviderTests
    {
        ClaimsOptions claimsOptions = new ClaimsOptions
        {
            Email = "email_claim_type",
            Role = "app_role_claim_type",
            LocationPrefix = "Location-",
            RolePrefix = "Role-"
        };

        [Fact]
        public void GetEmail()
        {
            // Arrange
            var logger = new Mock<ILogger<ClaimsProvider>>();

            var options = Options.Create(claimsOptions);
            var claimsProvider = new ClaimsProvider(options, logger.Object);
            var claimsPrincipal = new ClaimsPrincipal();
            claimsPrincipal.AddIdentity(new ClaimsIdentity(new List<Claim> {
                new Claim(claimsOptions.Email, "noreply@tts.test")
            }));

            // Act
            var emailClaimValue = claimsProvider.GetEmail(claimsPrincipal);

            // Assert
            Assert.Equal("noreply@tts.test", emailClaimValue);
        }

        [Fact]
        public void GetEmail_NotFound()
        {
            // Arrange
            var logger = new Mock<ILogger<ClaimsProvider>>();

            var options = Options.Create(claimsOptions);
            var claimsProvider = new ClaimsProvider(options, logger.Object);
            var claimsPrincipal = new ClaimsPrincipal();
            claimsPrincipal.AddIdentity(new ClaimsIdentity(new List<Claim> {
                new Claim("email_claim_type_different", "noreply@tts.test")
            }));

            // Act
            var email = claimsProvider.GetEmail(claimsPrincipal);

            // Assert
            Assert.Null(email);
        }

        /// <summary>
        /// Verify we can grab the location from the roles claim
        /// </summary>
        [Fact]
        public void GetState()
        {
            // Arrange
            var logger = new Mock<ILogger<ClaimsProvider>>();

            var options = Options.Create(claimsOptions);
            var claimsProvider = new ClaimsProvider(options, logger.Object);
            var claimsPrincipal = new ClaimsPrincipal();
            claimsPrincipal.AddIdentity(new ClaimsIdentity(new List<Claim> {
                new Claim(claimsOptions.Role, "Location-IA")
            }));

            // Act
            var state = claimsProvider.GetState(claimsPrincipal);

            // Assert
            Assert.Equal("IA", state);
        }

        /// <summary>
        /// Verify that if the roles claim is not found, we do not grab a location
        /// </summary>
        [Fact]
        public void GetState_RoleClaimNotFound()
        {
            // Arrange
            var logger = new Mock<ILogger<ClaimsProvider>>();

            var options = Options.Create(claimsOptions);
            var claimsProvider = new ClaimsProvider(options, logger.Object);
            var claimsPrincipal = new ClaimsPrincipal();
            claimsPrincipal.AddIdentity(new ClaimsIdentity(new List<Claim> {
                new Claim("app_role_claim_different", "Location-IA")
            }));

            // Act
            var state = claimsProvider.GetState(claimsPrincipal);

            // Assert
            Assert.Null(state);
        }

        /// <summary>
        /// Verify that if a roles claim does exist, but a location value does not exist, we do not grab a location
        /// </summary>
        [Fact]
        public void GetState_RoleClaimFound_LocationNotFound()
        {
            // Arrange
            var logger = new Mock<ILogger<ClaimsProvider>>();

            var options = Options.Create(claimsOptions);
            var claimsProvider = new ClaimsProvider(options, logger.Object);
            var claimsPrincipal = new ClaimsPrincipal();
            claimsPrincipal.AddIdentity(new ClaimsIdentity(new List<Claim> {
                new Claim(claimsOptions.Role, "Different-IA")
            }));

            // Act
            var state = claimsProvider.GetState(claimsPrincipal);

            // Assert
            Assert.Null(state);
        }

        /// <summary>
        /// When multiple locations exist, we just use the first one
        /// </summary>
        [Fact]
        public void GetStateWhenMultipleLocationsExist()
        {
            // Arrange
            var logger = new Mock<ILogger<ClaimsProvider>>();

            var options = Options.Create(claimsOptions);
            var claimsProvider = new ClaimsProvider(options, logger.Object);
            var claimsPrincipal = new ClaimsPrincipal();
            claimsPrincipal.AddIdentity(new ClaimsIdentity(new List<Claim> {
                new Claim(claimsOptions.Role, "Location-IA"),
                new Claim(claimsOptions.Role, "Location-MT")
            }));

            // Act
            var state = claimsProvider.GetState(claimsPrincipal);

            // Assert
            Assert.Equal("IA", state);
        }

        /// <summary>
        /// When multiple role claims exist, we should take the state from the one that starts with the NAC Location prefix
        /// </summary>
        [Fact]
        public void GetStateWhenMultipleRoleClaimsExist()
        {
            // Arrange
            var logger = new Mock<ILogger<ClaimsProvider>>();

            var options = Options.Create(claimsOptions);
            var claimsProvider = new ClaimsProvider(options, logger.Object);
            var claimsPrincipal = new ClaimsPrincipal();
            claimsPrincipal.AddIdentity(new ClaimsIdentity(new List<Claim> {
                new Claim(claimsOptions.Role, "OtherRole"),
                new Claim(claimsOptions.Role, "Location-IA")
            }));

            // Act
            var state = claimsProvider.GetState(claimsPrincipal);

            // Assert
            Assert.Equal("IA", state);
        }

        /// <summary>
        /// Verify we can grab the role from the claim
        /// </summary>
        [Fact]
        public void GetRole()
        {
            // Arrange
            var logger = new Mock<ILogger<ClaimsProvider>>();

            var options = Options.Create(claimsOptions);
            var claimsProvider = new ClaimsProvider(options, logger.Object);
            var claimsPrincipal = new ClaimsPrincipal();
            claimsPrincipal.AddIdentity(new ClaimsIdentity(new List<Claim> {
                new Claim(claimsOptions.Role, "Role-Worker")
            }));

            // Act
            var role = claimsProvider.GetRole(claimsPrincipal);

            // Assert
            Assert.Equal("Worker", role);
        }

        /// <summary>
        /// Verify if no role claim exists, we have no role
        /// </summary>
        [Fact]
        public void GetRole_RoleClaimNotFound()
        {
            // Arrange
            var logger = new Mock<ILogger<ClaimsProvider>>();

            var options = Options.Create(claimsOptions);
            var claimsProvider = new ClaimsProvider(options, logger.Object);
            var claimsPrincipal = new ClaimsPrincipal();
            claimsPrincipal.AddIdentity(new ClaimsIdentity(new List<Claim> {
                new Claim("app_role_claim_different", "Role-Worker")
            }));

            // Act
            var role = claimsProvider.GetRole(claimsPrincipal);

            // Assert
            Assert.Null(role);
        }

        /// <summary>
        /// Verify that if a role claim exists, but no Role exists, we have no Role
        /// </summary>
        [Fact]
        public void GetRole_RoleClaimFound_LocationNotFound()
        {
            // Arrange
            var logger = new Mock<ILogger<ClaimsProvider>>();

            var options = Options.Create(claimsOptions);
            var claimsProvider = new ClaimsProvider(options, logger.Object);
            var claimsPrincipal = new ClaimsPrincipal();
            claimsPrincipal.AddIdentity(new ClaimsIdentity(new List<Claim> {
                new Claim(claimsOptions.Role, "RoleDifferent-Worker")
            }));

            // Act
            var role = claimsProvider.GetRole(claimsPrincipal);

            // Assert
            Assert.Null(role);
        }

        /// <summary>
        /// When multiple Roles exist, we just use the first one
        /// </summary>
        [Fact]
        public void GetRoleWhenMultipleRolesExist()
        {
            // Arrange
            var logger = new Mock<ILogger<ClaimsProvider>>();

            var options = Options.Create(claimsOptions);
            var claimsProvider = new ClaimsProvider(options, logger.Object);
            var claimsPrincipal = new ClaimsPrincipal();
            claimsPrincipal.AddIdentity(new ClaimsIdentity(new List<Claim> {
                new Claim(claimsOptions.Role, "Role-Worker"),
                new Claim(claimsOptions.Role, "Role-OtherWorker")
            }));

            // Act
            var role = claimsProvider.GetRole(claimsPrincipal);

            // Assert
            Assert.Equal("Worker", role);
        }

        /// <summary>
        /// When multiple role claims exist, we should take the Role from the one that starts with the Role prefix
        /// </summary>
        [Fact]
        public void GetRoleWhenMultipleRoleClaimsExist()
        {
            // Arrange
            var logger = new Mock<ILogger<ClaimsProvider>>();

            var options = Options.Create(claimsOptions);
            var claimsProvider = new ClaimsProvider(options, logger.Object);
            var claimsPrincipal = new ClaimsPrincipal();
            claimsPrincipal.AddIdentity(new ClaimsIdentity(new List<Claim> {
                new Claim(claimsOptions.Role, "Location-IA"),
                new Claim(claimsOptions.Role, "Role-Worker")
            }));

            // Act
            var role = claimsProvider.GetRole(claimsPrincipal);

            // Assert
            Assert.Equal("Worker", role);
        }
    }
}