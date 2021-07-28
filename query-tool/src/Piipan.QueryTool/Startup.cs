using System.Diagnostics.CodeAnalysis;
using System.Net.Http;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using NEasyAuthMiddleware;
using Piipan.QueryTool.Binders;
using Piipan.Shared.Authentication;
using Piipan.Shared.Claims;
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
            services.Configure<ClaimsOptions>(Configuration.GetSection(ClaimsOptions.Claims));

            services.AddRazorPages().AddMvcOptions(options =>
            {
                options.ModelBinderProviders.Insert(0, new TrimModelBinderProvider());
            });

            services.AddSingleton<IAuthorizedApiClient>((s) =>
            {
                ITokenProvider tokenProvider;
                if (_env.IsDevelopment())
                {
                    tokenProvider = new CliTokenProvider();
                }
                else
                {
                    tokenProvider = new EasyAuthTokenProvider();
                }

                return new AuthorizedJsonApiClient(new HttpClient(), tokenProvider);
            });

            services.AddTransient<IClaimsProvider, ClaimsProvider>();

            services.AddHttpContextAccessor();
            services.AddEasyAuth();

            services.AddDistributedMemoryCache();
            services.AddSession();

            if (_env.IsDevelopment())
            {
                var mockFile = $"{_env.ContentRootPath}/mock_user.json";
                services.UseJsonFileToMockEasyAuth(mockFile);
            }
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
        }
    }
}
