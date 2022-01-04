// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace TeamsTabApp.Controllers
{
    using Microsoft.AspNetCore.Mvc;

    public class TabController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
