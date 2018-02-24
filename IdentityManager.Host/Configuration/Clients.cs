using System.Collections.Generic;
using IdentityServer4;
using IdentityServer4.Models;

namespace IdentityManager.Host.Configuration
{
    public class Clients
    {
        public static IEnumerable<Client> Get()
        {
            return new List<Client>
            {
                
                ///////////////////////////////////////////
                // JS OIDC Sample
                //////////////////////////////////////////
                new Client
                {
                    ClientId = "js_oidc2",
                    ClientName = "JavaScript OIDC Client",
                    ClientUri = "http://identityserver.io",
                    //LogoUri = "https://pbs.twimg.com/profile_images/1612989113/Ki-hanja_400x400.png",

                    AllowedGrantTypes = GrantTypes.Implicit,
                    AllowAccessTokensViaBrowser = true,
                    RequireClientSecret = false,
                    AccessTokenType = AccessTokenType.Jwt,
                    RequireConsent=false,
                    RedirectUris =
                    {
                        "http://localhost:7017/index.html",
                        "http://localhost:7017/callback.html",
                        "http://localhost:7017/silent.html",
                        "http://localhost:7017/popup.html",
                        "http://localhost:5001/idm#/callback/",
                        "http://localhost:5003/callback.html",
                    },

                    PostLogoutRedirectUris = { "http://localhost:5001/idm#/logout" },
                    AllowedCorsOrigins = { "http://localhost:5001","http://localhost:5003" },

                    AllowedScopes =
                    {
                        IdentityServerConstants.StandardScopes.OpenId,
                        IdentityServerConstants.StandardScopes.Profile,
                        IdentityServerConstants.StandardScopes.Email,
                        "api1", "api2.read_only", "api2.full_access",
                        "roles"
                    }
                },
                new Client
                {
                    ClientId = "js_oidc",
                    ClientName = "JavaScript OIDC Client",

                    AllowedGrantTypes = GrantTypes.Implicit,
                    AllowAccessTokensViaBrowser = true,
                    RequireClientSecret = false,
                    AccessTokenType = AccessTokenType.Jwt,
                    RequireConsent=false,

                    RedirectUris ={"http://localhost:5001/idm#/callback/","http://localhost:5003/callback.html"},
                    PostLogoutRedirectUris = { "http://localhost:5001/idm#", "http://localhost:5003/index.html" },
                    AllowedCorsOrigins = { "http://localhost:5001","http://localhost:5003" },
                    
                    AllowedScopes =
                    {
                        IdentityServerConstants.StandardScopes.OpenId,
                        IdentityServerConstants.StandardScopes.Profile,
                        IdentityServerConstants.StandardScopes.Email,
                        "api1", 
                        "roles"
                    }
                }
            };
        }
    }
}
