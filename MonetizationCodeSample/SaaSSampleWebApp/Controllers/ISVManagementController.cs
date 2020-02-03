// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace SaaSSampleWebApp.Controllers
{
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Mvc;

    /// <summary>
    /// isv admin page, the isv users can view all subscriptions details in the page 
    /// </summary>
    [Authorize("ISVAdmin")]
    public class ISVManagementController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}