using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using TzIdentityManager;
using TzIdentityManager.Configuration;
using TzIdentityManager.Core;
using TzIdentityServer.Configuration;
using TzIdentityServer.Data;
using TzIdentityServer.Models;
using TzIdentityServer.Services;

namespace TzIdentityServer
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddDbContext<ApplicationDbContext>(options =>
                options.UseSqlServer(Configuration.GetConnectionString("DefaultConnection")));

            services.AddIdentity<ApplicationUser, IdentityRole>(options=>
            {
                // example of setting options
                options.Tokens.ChangePhoneNumberTokenProvider = "Phone";
                
                // password settings chosen due to NIST SP 800-63
                options.Password.RequiredLength = 8; // personally i'd prefer to see 10+
                options.Password.RequiredUniqueChars = 0;
                options.Password.RequireDigit = false;
                options.Password.RequireLowercase = false;
                options.Password.RequireUppercase = false;
                options.Password.RequireNonAlphanumeric = false;
            })
            .AddEntityFrameworkStores<ApplicationDbContext>()
            .AddDefaultTokenProviders();

            // Add application services.
            services.AddTransient<IEmailSender, EmailSender>();

            services.AddMvc();

            // configure identity server with in-memory stores, keys, clients and scopes
            services.AddIdentityServer()
                .AddDeveloperSigningCredential()
                .AddInMemoryPersistedGrants()
                .AddInMemoryIdentityResources(Resources.GetIdentityResources())
                .AddInMemoryApiResources(Resources.GetApiResources())
                .AddInMemoryClients(Clients.Get())
                .AddAspNetIdentity<ApplicationUser>();

            services.AddAuthentication()
                .AddGoogle(options =>
                {
                    options.ClientId = "998042782978-s07498t8i8jas7npj4crve1skpromf37.apps.googleusercontent.com";
                    options.ClientSecret = "HsnwJri_53zn7VcO1Fm7THBb";
                });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseBrowserLink();
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
                    SecurityConfiguration = new HostSecurityConfiguration
                    {
                        RequireSsl = true,
                        ShowLoginButton = true,
                        BearerAuthenticationType = "Bearer",
                        AdminRoleName = "admin",
                        RoleClaimType = "role",
                        Authority = "http://localhost:5000",
                        ClientId = "js_oidc",
                        Scope = "openid profile email api1",
                        ResponseType = "id_token token",
                        ApiName = "api1",

                        AuthorizationPolicy = new AuthorizationPolicyBuilder()
                            .AddAuthenticationSchemes(Constants.BearerAuthenticationType)
                            .RequireRole("admin")
                            .RequireAuthenticatedUser()
                            .Build()
                    },
                });

            app.UseStaticFiles();
            app.UseStaticFiles(new StaticFileOptions
            {
                ServeUnknownFileTypes = true, // for letsencrypt sertificate generation
            });

            // app.UseAuthentication(); // not needed, since UseIdentityServer adds the authentication middleware
            app.UseIdentityServer();
            app.UseMvcWithDefaultRoute();
        }
    }
}
