// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace SaaSSampleWebApp.Controllers
{
    using System.Threading.Tasks;

    using Microsoft.AspNetCore.Mvc;
    using SaaSSampleWebApp.Services;

    /// <summary>
    /// fulfilment page
    /// </summary>
    public class FulfilmentController : Controller
    {
        private readonly IMSGraphHelper _msGraphHelper;

        public FulfilmentController(IMSGraphHelper msGraphHelper)
        {
            _msGraphHelper = msGraphHelper;
        }

        public async Task<IActionResult> Index()
        {
            if (await _msGraphHelper.IsTenantAdminAsync())
            {
                if (Request.Query != null && !string.IsNullOrEmpty(Request.Query["token"]))
                {
                    ViewData["Section"] = "redirectFromAppSource";
                }
                else
                {
                    ViewData["Section"] = "firstLandingSection";
                }
            }

            return View();
        }
    }
}