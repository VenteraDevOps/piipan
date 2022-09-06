using System;
using System.Diagnostics.CodeAnalysis;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using NEasyAuthMiddleware;
using Piipan.Components.Modals;
using Piipan.Components.Routing;
using Piipan.Match.Client.Extensions;
using Piipan.QueryTool.Binders;
using Piipan.Shared.Authorization;
using Piipan.Shared.Claims;
using Piipan.Shared.Deidentification;
using Piipan.Shared.Locations;
using Piipan.Shared.Logging;
using Piipan.Shared.Roles;
using Piipan.States.Client.Extensions;

namespace Piipan.QueryTool
{
    [ExcludeFromCodeCoverage]
    public class Startup
    {
        public Startup(IConfiguration configuration, IWebHostEnvironment env)
        {
            Configuration = configuration;
            _env = env;
        }

        public IConfiguration Configuration { get; }
        private readonly IWebHostEnvironment _env;

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.Configure<LocationOptions>(Configuration.GetSection(LocationOptions.SectionName));
            services.Configure<RoleOptions>(Configuration.GetSection(RoleOptions.SectionName));
            services.Configure<ClaimsOptions>(Configuration.GetSection(ClaimsOptions.SectionName));


            services.Configure<ForwardedHeadersOptions>(options =>
            {
                options.ForwardedHeaders =
                    ForwardedHeaders.XForwardedHost | ForwardedHeaders.XForwardedProto;
            });

            services.AddRazorPages(options =>
            {
                options.Conventions.AuthorizeFolder("/");
                options.Conventions.AllowAnonymousToPage("/SignedOut");
            }).AddMvcOptions(options =>
            {
                options.ModelBinderProviders.Insert(0, new TrimModelBinderProvider());
            });

            services.AddHttpClient();

            services.AddHsts(options =>
            {
                options.Preload = true; //Sets the preload parameter of the Strict-Transport-Security header. Preload isn't part of the RFC HSTS specification, but is supported by web browsers to preload HSTS sites on fresh install. For more information, see https://hstspreload.org
                options.IncludeSubDomains = true; //Enables includeSubDomain, which applies the HSTS policy to Host subdomains.
                options.MaxAge = TimeSpan.FromSeconds(31536000); //Explicitly sets the max-age parameter of the Strict-Transport-Security header to 365 days. If not set, defaults to 30 days.
            });

            services.AddTransient<IClaimsProvider, ClaimsProvider>();
            services.AddTransient<ILocationsProvider, LocationsProvider>();
            services.AddTransient<IRolesProvider, RolesProvider>();

            services.AddSingleton<INameNormalizer, NameNormalizer>();
            services.AddSingleton<IDobNormalizer, DobNormalizer>();
            services.AddSingleton<ISsnNormalizer, SsnNormalizer>();
            services.AddSingleton<ILdsHasher, LdsHasher>();
            services.AddSingleton<ILdsDeidentifier, LdsDeidentifier>();

            services.AddModalManager();
            services.AddPiipanNavigationManager();

            services.AddHttpContextAccessor();
            services.AddEasyAuth();

            services.AddDistributedMemoryCache();
            services.AddSession();

            services.AddAuthorizationCore(options =>
            {
                options.DefaultPolicy = AuthorizationPolicyBuilder.Build(Configuration
                    .GetSection(AuthorizationPolicyOptions.SectionName)
                    .Get<AuthorizationPolicyOptions>());

            });

            services.RegisterMatchClientServices(_env);
            services.RegisterMatchResolutionClientServices(_env);
            services.RegisterStatesClientServices(_env);

            if (_env.IsDevelopment())
            {
                var mockFile = $"{_env.ContentRootPath}/mock_user.json";
                services.UseJsonFileToMockEasyAuth(mockFile);
            }

            services.AddAntiforgery(options =>
            {
                options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
            });

            services.AddApplicationInsightsTelemetry((options) =>
            {
                options.ConnectionString = Environment.GetEnvironmentVariable("APPLICATIONINSIGHTS_CONNECTION_STRING");
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Error");
                app.UseForwardedHeaders();
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseBlazorFrameworkFiles();
            app.UseStatusCodePagesWithReExecute("/NotAuthorized", "?code={0}");

            //Perform middleware for custom 404 page
            app.Use(async (context, next) =>
            {
                try
                {
                    context.Response.Headers.Add("X-Frame-Options", "DENY");
                    await next();
                    if (context.Response.StatusCode == 403 &&
                        (!context.Request.Path.Value?.TrimEnd('/').TrimEnd('\\').EndsWith("NotAuthorized", StringComparison.InvariantCultureIgnoreCase) ?? true))
                    {
                        context.Response.Redirect("/NotAuthorized");
                    }
                    if (context.Response.StatusCode == 404 || context.Response.StatusCode == 500)
                    {
                        context.Request.Path = "/Error";
                        context.Response.StatusCode = 200;
                        await next();
                    }
                }
                catch
                {
                    context.Request.Path = "/Error";
                    context.Response.StatusCode = 200;
                    await next();
                }
            });
            app.UseStaticFiles();

            app.UseAuthentication();
            app.UseRouting();

            app.UseSession();

            app.UseAuthorization();

            app.UseMiddleware<RequestLoggingMiddleware>();
            app.UseMiddleware<AuthenticationLoggingMiddleware>();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapRazorPages();
                endpoints.MapFallbackToPage("/Error");
            });
        }
    }
}
