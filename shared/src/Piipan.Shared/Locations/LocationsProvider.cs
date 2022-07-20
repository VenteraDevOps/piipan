using System;
using Microsoft.Extensions.Options;

namespace Piipan.Shared.Locations
{
    public class LocationsProvider : ILocationsProvider
    {
        private readonly LocationOptions _options;

        public LocationsProvider(IOptions<LocationOptions> options)
        {
            _options = options.Value;
        }

        public string[] GetStates(string location)
        {
            if (location == _options.NationalOfficeValue)
            {
                return new string[] { "*" };
            }
            // TODO: Fetch states from State Func API and cache. Return string[] with the states matching our region
            if (!string.IsNullOrEmpty(location))
            {
                return new string[] { location };
            }
            return Array.Empty<string>();
        }
    }
}
