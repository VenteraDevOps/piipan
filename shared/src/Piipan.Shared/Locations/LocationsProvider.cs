using System;
using System.Linq;
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
            var stateArray = _options.Map?.FirstOrDefault(n => n.Name == location)?.States?.ToArray();
            if (stateArray == null && !string.IsNullOrEmpty(location))
            {
                return new string[] { location };
            }
            return stateArray ?? Array.Empty<string>();
        }
    }
}
