using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Piipan.QueryTool.Client.Models;
using Piipan.Shared.Claims;
using Piipan.Shared.Locations;
using Piipan.States.Api;
using Piipan.States.Api.Models;

namespace Piipan.QueryTool.Pages
{
    public class BasePageModel : PageModel
    {
        private const string NotAuthorizedPageName = "/NotAuthorized";
        private const string StateInfoCacheName = "StateInfo";
        private readonly IClaimsProvider _claimsProvider;
        private readonly ILocationsProvider _locationsProvider;
        private readonly IMemoryCache _memoryCache;
        private readonly IStatesApi _statesApi;
        private readonly IConfiguration _configuration;

        public BasePageModel(IServiceProvider serviceProvider)
        {
            _claimsProvider = serviceProvider.GetRequiredService<IClaimsProvider>();
            _locationsProvider = serviceProvider.GetRequiredService<ILocationsProvider>();
            _statesApi = serviceProvider.GetRequiredService<IStatesApi>();
            _memoryCache = serviceProvider.GetRequiredService<IMemoryCache>();
            _configuration = serviceProvider.GetRequiredService<IConfiguration>();

            AppData.HelpDeskEmail = _configuration.GetValue<string>("HelpDeskEmail");
        }

        public override async Task OnPageHandlerExecutionAsync(PageHandlerExecutingContext context, PageHandlerExecutionDelegate next)
        {
            // If there are no states or it's null, let's try to fetch it again.
            if (StateInfo?.Results?.Count() == 0)
            {
                _memoryCache.Remove(StateInfoCacheName);
            }
            StateInfo = await _memoryCache.GetOrCreateAsync(StateInfoCacheName, async (e) =>
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

        protected IActionResult UnauthorizedResult()
        {
            AppData.IsAuthorized = false;
            return Page();
        }

        protected IActionResult RedirectToUnauthorized()
        {
            return RedirectToPage(NotAuthorizedPageName);
        }

        public string Email
        {
            get { return _claimsProvider.GetEmail(User); }
        }

        public string Location
        {
            get { return _claimsProvider.GetLocation(User); }
        }

        public string[] States
        {
            get { return _locationsProvider.GetStates(Location); }
        }

        public bool IsNationalOffice => States?.Contains("*") ?? false;

        public string Role
        {
            get { return _claimsProvider.GetRole(User); }
        }
        public string BaseUrl
        {
            get { return $"{Request.Scheme}://{Request.Host}"; }
        }

        public StatesInfoResponse StateInfo { get; set; }

        public AppData AppData { get; } = new AppData();
    }
}