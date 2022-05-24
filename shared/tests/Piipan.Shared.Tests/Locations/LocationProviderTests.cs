using Microsoft.Extensions.Options;
using Xunit;

namespace Piipan.Shared.Locations.Tests
{
    public class LocationProviderTests
    {
        LocationOptions locationOptions = new LocationOptions
        {
            Map = new LocationMapping[]
            {
                new LocationMapping { Name = "National", States = new[] { "*" }},
                new LocationMapping { Name = "Midwest", States = new[] { "WI", "IA", "MN" }},
            }
        };

        /// <summary>
        /// Get the
        /// </summary>
        [Fact]
        public void GetNationalStates()
        {
            // Arrange
            var options = Options.Create(locationOptions);
            var locationProvider = new LocationsProvider(options);

            // Act
            var states = locationProvider.GetStates("National");

            // Assert
            Assert.Equal(new string[] { "*" }, states);
        }

        [Fact]
        public void GetMidwestStates()
        {
            // Arrange
            var options = Options.Create(locationOptions);
            var locationProvider = new LocationsProvider(options);

            // Act
            var states = locationProvider.GetStates("Midwest");

            // Assert
            Assert.Equal(new string[] { "WI", "IA", "MN" }, states);
        }

        [Fact]
        public void GetSingleState()
        {
            // Arrange
            var options = Options.Create(locationOptions);
            var locationProvider = new LocationsProvider(options);

            // Act
            var states = locationProvider.GetStates("IA");

            // Assert
            Assert.Equal(new string[] { "IA" }, states);
        }

        [Fact]
        public void GetNullState()
        {
            // Arrange
            var options = Options.Create(locationOptions);
            var locationProvider = new LocationsProvider(options);

            // Act
            var states = locationProvider.GetStates(null);

            // Assert
            Assert.Empty(states);
        }
    }
}