using System;
using System.Security.Claims;
using AutoMapper;
using IdentityServer4.AccessTokenValidation;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Hosting.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Authorization;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using TzIdentityManager.Api.Models.AutoMapper;

namespace TzIdentityManager.Configuration
{
    public static class AppBuilderExtensions
    {


        public static void UseIdentityManager(this IApplicationBuilder appBuilder, Action<IServiceCollection> registerServices, IdentityManagerOptions identityManagerOptions)
        {
            if (appBuilder == null) throw new ArgumentNullException(nameof(appBuilder));
            if (identityManagerOptions == null) throw new ArgumentNullException(nameof(identityManagerOptions));

            //IIdentityManagerService identityManagerService = null;
            //var scopeFactory = appBuilder.ApplicationServices.GetRequiredService<IServiceScopeFactory>();
            //using (var scope = scopeFactory.CreateScope())
            //{
            //    identityManagerService = scope.ServiceProvider.GetRequiredService<IIdentityManagerService>();
            //}

            Action<IServiceCollection> branchServices = idmServices =>
            {
                registerServices?.Invoke(idmServices);

                //idmServices.AddTransient(branchBuilder => identityManagerService);

                idmServices.Configure<IdentityManagerOptions>(o =>
                {
                    o.SecurityConfiguration = identityManagerOptions.SecurityConfiguration;
                    o.DisableUserInterface = identityManagerOptions.DisableUserInterface;
                });

                idmServices.AddAutoMapper(typeof(IdentityModelProfile));

                idmServices.AddMvc(opt =>
                {
                    if (identityManagerOptions.SecurityConfiguration.RequireSsl)
                    {
                        opt.Filters.Add(new RequireHttpsAttribute());
                    }
                    opt.Filters.Add(new AuthorizeFilter(identityManagerOptions.SecurityConfiguration.AuthorizationPolicy));
                });

                idmServices.AddAuthentication(IdentityServerAuthenticationDefaults.AuthenticationScheme)
                    .AddIdentityServerAuthentication(option =>
                    {
                        option.Authority = identityManagerOptions.SecurityConfiguration.Authority;
                        option.RoleClaimType = identityManagerOptions.SecurityConfiguration.RoleClaimType;// "role";
                        option.RequireHttpsMetadata = false;
                        option.ApiName = identityManagerOptions.SecurityConfiguration.ApiName;// "api1";
                    });
            };

            appBuilder.UseBranchWithServices("/idm",branchServices,
                app => {

                  
                    //options.Value.SecurityConfiguration.Configure(app);

                    app.UseFileServer(new FileServerOptions
                    {
                        FileProvider = new EmbeddedFileProvider(typeof(SecurityConfiguration).Assembly,
                            "TzIdentityManager.Assets"),
                        RequestPath = new PathString("/assets"),
                    });
                    app.UseFileServer(new FileServerOptions
                    {
                        FileProvider = new EmbeddedFileProvider(typeof(SecurityConfiguration).Assembly,
                            "TzIdentityManager.Assets.Content.fonts"),
                        RequestPath = new PathString("/assets/libs/fonts"),
                    });

                    app.UseStaticFiles();


                    //Suppress Default Host Authentication;
                    app.Use(async (ctx, next) =>
                    {
                        ctx.User = new ClaimsPrincipal(new ClaimsIdentity());
                        await next.Invoke();
                    });

                    app.UseAuthentication();

                    app.UseMvc(routes =>
                    {
                        routes.MapRoute(Constants.RouteNames.Home, "{controller=Page}/{action=Index}");
                        routes.MapRoute(Constants.RouteNames.Logout, "{controller=Page}/{action=Logout}");
                    });
                    //app.UseStageMarker(PipelineStage.MapHandler);  
                }
            );
            







           

        }










        public static IApplicationBuilder UseBranchWithServices(this IApplicationBuilder app, PathString path,
                Action<IServiceCollection> servicesConfiguration, Action<IApplicationBuilder> appBuilderConfiguration)
            {
                
                var webHost = new WebHostBuilder().UseKestrel().ConfigureServices(servicesConfiguration).UseStartup<EmptyStartup>().Build();
                var serviceProvider = webHost.Services;
                var serverFeatures = webHost.ServerFeatures;

                var appBuilderFactory = serviceProvider.GetRequiredService<IApplicationBuilderFactory>();
                var branchBuilder = appBuilderFactory.CreateBuilder(serverFeatures);
                var factory = serviceProvider.GetRequiredService<IServiceScopeFactory>();

                branchBuilder.Use(async (context, next) =>
                {
                    using (var scope = factory.CreateScope())
                    {
                        context.RequestServices = scope.ServiceProvider;
                        await next();
                    }
                });

                appBuilderConfiguration(branchBuilder);

                var branchDelegate = branchBuilder.Build();

                return app.Map(path, builder =>
                {
                    builder.Use(async (context, next) =>
                    {
                        await branchDelegate(context);
                    });
                });
            }

            private class EmptyStartup
            {
                public void ConfigureServices(IServiceCollection services) { }

                public void Configure(IApplicationBuilder app) { }
            }
        


    }
}
