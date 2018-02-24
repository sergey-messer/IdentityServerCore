using System;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using TzIdentityManager.Api.Filters;
using TzIdentityManager.Assets;
using TzIdentityManager.Configuration;

namespace TzIdentityManager.Api.Controllers
{
    [ResponseCache(Location = ResponseCacheLocation.None, NoStore = true)]
    [SecurityHeaders]
    public class PageController : Controller
    {
        readonly IdentityManagerOptions _idmConfig;
        public PageController(IOptions<IdentityManagerOptions> idmConfig)
        {
            if (idmConfig == null) throw new ArgumentNullException(nameof(idmConfig));

            this._idmConfig = idmConfig.Value;
        }

        [HttpGet]
        [AllowAnonymous]
        public IActionResult Index()
        {
            return new EmbeddedHtmlResult(Request, "TzIdentityManager.Assets.Templates.index.html", _idmConfig.SecurityConfiguration);
        }

        [HttpGet]
        [AllowAnonymous]
        public IActionResult Logout()
        {
            //idmConfig.SecurityConfiguration.SignOut(Request.GetOwinContext());
            return RedirectToRoute(Constants.RouteNames.Home, null);
        }
    }
}
