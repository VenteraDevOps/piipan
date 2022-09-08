using System;
using Microsoft.Extensions.Options;

namespace Piipan.Shared.Roles
{
    /// <summary>
    /// The RolesProvider will provide methods for the server to determine if a user has the roles to perform a function.
    /// The server can then pass these roles onto the Client so that the client can show/hide certain areas of the screen.
    /// </summary>
    public class RolesProvider : IRolesProvider
    {
        private readonly RoleOptions _options;

        public RolesProvider(IOptions<RoleOptions> options)
        {
            _options = options.Value;
        }

        /// <summary>
        /// Returns which roles are acceptable when editing a match.
        /// </summary>
        /// <returns></returns>
        public string[] GetMatchEditRoles()
        {
            return _options?.EditMatch ?? Array.Empty<string>();
        }

        /// <summary>
        /// Returns which roles are acceptable when viewing a match.
        /// </summary>
        /// <returns></returns>
        public string[] GetMatchViewRoles()
        {
            return _options?.ViewMatch ?? Array.Empty<string>();
        }
    }
}
