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
            NACLocationPrefix = "Location-",
            NACRolePrefix = "Role-"
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
        /// Verify we can grab the NAC role from the claim
        /// </summary>
        [Fact]
        public void GetNACRole()
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
            var nacRole = claimsProvider.GetNACRole(claimsPrincipal);

            // Assert
            Assert.Equal("Worker", nacRole);
        }

        /// <summary>
        /// Verify if no role claim exists, we have no NAC Role
        /// </summary>
        [Fact]
        public void GetNACRole_RoleClaimNotFound()
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
            var nacRole = claimsProvider.GetNACRole(claimsPrincipal);

            // Assert
            Assert.Null(nacRole);
        }

        /// <summary>
        /// Verify that if a role claim exists, but no NAC Role exists, we have no NAC Role
        /// </summary>
        [Fact]
        public void GetNACRole_RoleClaimFound_LocationNotFound()
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
            var nacRole = claimsProvider.GetNACRole(claimsPrincipal);

            // Assert
            Assert.Null(nacRole);
        }

        /// <summary>
        /// When multiple NAC Roles exist, we just use the first one
        /// </summary>
        [Fact]
        public void GetNACRoleWhenMultipleNACRolesExist()
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
            var nacRole = claimsProvider.GetNACRole(claimsPrincipal);

            // Assert
            Assert.Equal("Worker", nacRole);
        }

        /// <summary>
        /// When multiple role claims exist, we should take the NAC Role from the one that starts with the NAC Role prefix
        /// </summary>
        [Fact]
        public void GetNACRoleWhenMultipleRoleClaimsExist()
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
            var nacRole = claimsProvider.GetNACRole(claimsPrincipal);

            // Assert
            Assert.Equal("Worker", nacRole);
        }
    }
}