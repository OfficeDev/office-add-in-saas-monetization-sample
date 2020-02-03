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
    /// license user page, the license admin can assign license for the specific user in the page.
    /// </summary>
    [Authorize]
    public class UserManagementController : Controller
    {
        private readonly ISubscriptionService _subscriptionService;

        public UserManagementController(ISubscriptionService subscriptionService)
        {
            _subscriptionService = subscriptionService;
        }

        public async Task<IActionResult> Index()
        {
            var subscription = await _subscriptionService
                .GetSubscriptionAsync(Guid.Parse(User.GetTenantId()));

            if (subscription != null)
            {
                ViewData["subscriptionId"] = subscription.Id;
                return View();
            }

            return Redirect("/fulfilment");
        }
    }
}