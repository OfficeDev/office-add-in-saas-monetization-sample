// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace SaaSSampleWebApp.Components
{
    using System.Threading.Tasks;

    using Microsoft.AspNetCore.Mvc;
    using SaaSSampleWebApp.Services;

    public class UserViewComponent: ViewComponent
    {
        private readonly IMSGraphHelper _msGraphHelper;

        public UserViewComponent(IMSGraphHelper msGraphHelper)
        {
            _msGraphHelper = msGraphHelper;
        }

        public async Task<IViewComponentResult> InvokeAsync(string IdentityName)
        {
            var me = await _msGraphHelper.GetUserAsync(IdentityName);

            return View(me);
        }
    }
}
