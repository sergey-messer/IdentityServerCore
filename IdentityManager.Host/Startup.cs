﻿using IdentityManager.Host.Configuration;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using IdentityManager.Host.Data;
using IdentityManager.Host.Models;
using IdentityManager.Host.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.DependencyInjection.Extensions;
using TzIdentityManager;
using TzIdentityManager.Configuration;
using TzIdentityManager.Core;

namespace IdentityManager.Host
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddDbContext<ApplicationDbContext>(options =>
                options.UseSqlServer(Configuration.GetConnectionString("DefaultConnection")));

            services.AddIdentity<ApplicationUser, IdentityRole>()
                .AddEntityFrameworkStores<ApplicationDbContext>()
                .AddDefaultTokenProviders();

            services.AddTransient<IEmailSender, EmailSender>();
            
            services.AddMvc();

            services.AddIdentityServer()
                .AddDeveloperSigningCredential()
                .AddInMemoryPersistedGrants()
                .AddInMemoryIdentityResources(Resources.GetIdentityResources())
                .AddInMemoryApiResources(Resources.GetApiResources())
                .AddInMemoryClients(Clients.Get())
                .AddAspNetIdentity<ApplicationUser>();

            //services.AddAuthentication();
           
        }

        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseBrowserLink();
                app.UseDeveloperExceptionPage();
                app.UseDatabaseErrorPage();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
            }



            app.UseIdentityManager(
                idmServices =>
                {
                    idmServices.AddDbContext<ApplicationDbContext>(options =>
                        options.UseSqlServer(Configuration.GetConnectionString("DefaultConnection")));

                    idmServices.AddIdentity<ApplicationUser, IdentityRole>()
                        .AddEntityFrameworkStores<ApplicationDbContext>()
                        .AddDefaultTokenProviders();

                    idmServices.AddTransient
                        <IIdentityManagerService,
                            AspNetCoreIdentityManagerService<ApplicationUser, IdentityRole>>();
                }
                , 
                new IdentityManagerOptions()
                {
                    SecurityConfiguration = new HostSecurityConfiguration{
                        RequireSsl = false,
                        ShowLoginButton = true,
                        BearerAuthenticationType ="Bearer",
                        AdminRoleName = "admin",
                        RoleClaimType = "role",
                        Authority =  "http://localhost:5001",
                        ClientId = "js_oidc",
                        Scope = "openid profile email api1",
                        ResponseType= "id_token token",
                        ApiName= "api1",

                        AuthorizationPolicy = new AuthorizationPolicyBuilder()
                            .AddAuthenticationSchemes(Constants.BearerAuthenticationType)
                            .RequireRole("admin")
                            .RequireAuthenticatedUser()
                            .Build()
                    },
                });

            
            app.UseStaticFiles();
            app.UseIdentityServer();
            app.UseMvcWithDefaultRoute();
        }
    }
}
