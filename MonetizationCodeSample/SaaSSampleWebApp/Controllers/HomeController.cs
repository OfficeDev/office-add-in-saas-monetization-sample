// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace SaaSSampleWebApp.Controllers
{
    using System;
    using System.Diagnostics;
    using System.Threading.Tasks;

    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.Identity.Web;
    using SaaSSampleWebApp.Models;
    using SaaSSampleWebApp.Services;

    /// <summary>
    /// home page
    /// </summary>
    [AllowAnonymous]
    public class HomeController : Controller
    {
        private readonly IMSGraphHelper _msGraphHelper;
        private readonly ISubscriptionService _subscriptionService;

        public HomeController(IMSGraphHelper msGraphHelper, ISubscriptionService subscriptionService)
        {
            _subscriptionService = subscriptionService;
            _msGraphHelper = msGraphHelper;
        }

        public async Task<IActionResult> Index()
        {
            if (User.Identity.IsAuthenticated)
            {
                var subscription = await _subscriptionService
                    .GetSubscriptionAsync(Guid.Parse(User.GetTenantId()));

                if (subscription != null)
                {
                    ViewData["LicenseType"] = subscription.LicenceType;

                    if (await _msGraphHelper.IsTenantAdminAsync())
                    {
                        ViewData["Role"] = "LicenseAdmin";
                    }
                    else
                    {
                        var licenseManager = await _subscriptionService
                            .CheckLicenseManagerAsync(subscription.Id, Guid.Parse(User.GetObjectId()));

                        if (licenseManager == null)
                        {
                            ViewData["Role"] = "User";
                        }
                        else if (licenseManager.IsAdmin)
                        {
                            ViewData["Role"] = "LicenseAdmin";
                        }
                        else
                        {
                            ViewData["Role"] = "LicenseUser";
                        }
                    }
                }
                else
                {
                    return Redirect("/Fulfilment");
                }
            }

            return View();
        }

        public IActionResult SPHostedAddinEmbedContent()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel 
            { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
