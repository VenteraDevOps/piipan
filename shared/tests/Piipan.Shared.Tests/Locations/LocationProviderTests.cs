using Microsoft.Extensions.Options;
using Xunit;

namespace Piipan.Shared.Locations.Tests
{
    public class LocationProviderTests
    {
        LocationOptions locationOptions = new LocationOptions
        {
            NationalOfficeValue = "National"
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

        // TODO: Add regions back in after State Func API is complete
        //[Fact]
        //public void GetMidwestStates()
        //{
        //    // Arrange
        //    var options = Options.Create(locationOptions);
        //    var locationProvider = new LocationsProvider(options);

        //    // Act
        //    var states = locationProvider.GetStates("Midwest");

        //    // Assert
        //    Assert.Equal(new string[] { "WI", "IA", "MN" }, states);
        //}

        [Fact]
        public void GetSingleState()
        {
            // Arrange
            var options = Options.Create(locationOptions);
            var locationProvider = new LocationsProvider(options);

            // Act
            var states = locationProvider.GetStates("EA");

            // Assert
            Assert.Equal(new string[] { "EA" }, states);
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