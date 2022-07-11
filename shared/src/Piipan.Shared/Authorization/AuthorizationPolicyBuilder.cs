using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Authorization;

namespace Piipan.Shared.Authorization
{
    public static class AuthorizationPolicyBuilder
    {
        public static AuthorizationPolicy Build(AuthorizationPolicyOptions options)
        {
            var innerBuilder = new Microsoft.AspNetCore.Authorization.AuthorizationPolicyBuilder();

            // if no authorization policy is configured, forbid all requests
            if (options is null)
            {
                innerBuilder.RequireAssertion(context => false);
            }
            else
            {
                foreach (var rcv in options.RequiredClaims)
                {
                    // if the allowed values includes a wildcard, allow any value for this claim type
                    if (rcv.Values.Any(v => v == "*"))
                    {
                        innerBuilder.RequireAssertion(context => {
                            return context.User.Claims.Any(c => c.Type == rcv.Type);
                        });
                    }
                    else
                    {
                        innerBuilder.RequireClaim(rcv.Type, rcv.Values);
                    }
                }
            }
            
            innerBuilder.RequireAuthenticatedUser();

            return innerBuilder.Build();
        }
    }
}