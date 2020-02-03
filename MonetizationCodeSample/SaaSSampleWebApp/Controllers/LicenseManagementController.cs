// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace SaaSSampleWebApp.Controllers
{
    using System;
    using System.Threading.Tasks;

    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.Identity.Web;
    using SaaSSampleWebApp.Services;

    /// <summary>
    /// license management page, the subscriber can manage user in the page.
    /// </summary>
    [Authorize]
    public class LicenseManagementController : Controller
    {
        private readonly IMSGraphHelper _msGraphHelper;
        private readonly ISubscriptionService _subscriptionService;

        public LicenseManagementController(ISubscriptionService subscriptionService, IMSGraphHelper msGraphHelper)
        {
            _subscriptionService = subscriptionService;
            _msGraphHelper = msGraphHelper;
        }

        public async Task<IActionResult> Index()
        {
            var subscription = await _subscriptionService
                .GetSubscriptionAsync(Guid.Parse(User.GetTenantId()));

            if (subscription == null) return Redirect("/Fulfilment");

            ViewData["subscriptionId"] = subscription.Id;

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

            return View();
        }
    }
}