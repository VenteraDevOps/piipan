using System;
using System.Diagnostics.CodeAnalysis;
using System.Net;
using System.Net.Http;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using NEasyAuthMiddleware;
using Piipan.Match.Api;
using Piipan.Match.Client.Extensions;
using Piipan.QueryTool.Binders;
using Piipan.Shared.Authentication;
using Piipan.Shared.Authorization;
using Piipan.Shared.Claims;
using Piipan.Shared.Deidentification;
using Piipan.Shared.Logging;

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
            services.Configure<ClaimsOptions>(Configuration.GetSection(ClaimsOptions.SectionName));

            services.Configure<ForwardedHeadersOptions>(options => {
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

            services.AddHsts(options =>
            {
                options.Preload = true;
                options.IncludeSubDomains = true;
                options.MaxAge = TimeSpan.FromSeconds(31536000);
                // options.ExcludedHosts.Add("example.com");
                // options.ExcludedHosts.Add("www.example.com");
            });

            services.AddHttpsRedirection(options =>
            {
                options.RedirectStatusCode = (int) HttpStatusCode.TemporaryRedirect;
                options.HttpsPort = 5001;
            });

            services.AddTransient<IClaimsProvider, ClaimsProvider>();
            
            services.AddSingleton<INameNormalizer, NameNormalizer>();
            services.AddSingleton<IDobNormalizer, DobNormalizer>();
            services.AddSingleton<ISsnNormalizer, SsnNormalizer>();
            services.AddSingleton<ILdsHasher, LdsHasher>();
            services.AddSingleton<ILdsDeidentifier, LdsDeidentifier>();

            services.AddHttpContextAccessor();
            services.AddEasyAuth();

            services.AddDistributedMemoryCache();
            services.AddSession();

            services.AddAuthorizationCore(options => {
                options.DefaultPolicy = AuthorizationPolicyBuilder.Build(Configuration
                    .GetSection(AuthorizationPolicyOptions.SectionName)
                    .Get<AuthorizationPolicyOptions>());
            });

            

            services.RegisterMatchClientServices(_env);

            if (_env.IsDevelopment())
            {
                var mockFile = $"{_env.ContentRootPath}/mock_user.json";
                services.UseJsonFileToMockEasyAuth(mockFile);
            }

            services.AddAntiforgery(options => {
                options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
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
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
                app.UseForwardedHeaders();
            }

            app.UseHttpsRedirection();
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
            });

            app.Use(async (context, next) =>
            {
                context.Response.Headers.Add("X-Frame-Options", "DENY");
                await next();
            });
        }
    }
}
