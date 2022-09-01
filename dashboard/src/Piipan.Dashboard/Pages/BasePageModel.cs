using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using Piipan.Dashboard.Client.Models;
using Piipan.Shared.Claims;
using Piipan.States.Api;
using Piipan.States.Api.Models;

namespace Piipan.Dashboard.Pages
{
    public class BasePageModel : PageModel
    {
        private const string StateInfoCacheName = "StateInfo";
        private readonly IClaimsProvider _claimsProvider;
        private readonly IMemoryCache _memoryCache;
        private readonly IStatesApi _statesApi;

        public BasePageModel(IServiceProvider serviceProvider)
        {
            _claimsProvider = serviceProvider.GetRequiredService<IClaimsProvider>();
            _statesApi = serviceProvider.GetRequiredService<IStatesApi>();
            _memoryCache = serviceProvider.GetRequiredService<IMemoryCache>();

        }

        public string Email
        {
            get { return _claimsProvider.GetEmail(User); }
        }
        public string BaseUrl
        {
            get { return $"{Request.Scheme}://{Request.Host}"; }
        }

        public AppData AppData { get; } = new AppData();

        public override async Task OnPageHandlerExecutionAsync(PageHandlerExecutingContext context, PageHandlerExecutionDelegate next)
        {
            // If there are no states or it's null, let's try to fetch it again.
            if (AppData.StateInfo?.Results?.Count() == 0)
            {
                _memoryCache.Remove(StateInfoCacheName);
            }
            AppData.StateInfo = await _memoryCache.GetOrCreateAsync(StateInfoCacheName, async (e) =>
            {
                try
                {
                    return await _statesApi.GetStates();
                }
                catch
                {
                    // If an error occurs while fetching the states just return an empty enumerable
                    return new StatesInfoResponse { Results = Enumerable.Empty<StateInfoResponseData>() };
                }
            });

            await next();
        }
    }
}