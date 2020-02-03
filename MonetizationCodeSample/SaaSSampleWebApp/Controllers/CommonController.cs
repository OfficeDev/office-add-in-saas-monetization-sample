// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace SaaSSampleWebApp.Controllers
{
    using System;
    using System.Threading.Tasks;

    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.Extensions.Options;
    using Microsoft.Identity.Web;
    using SaaSSampleWebApp.Extensions;
    using SaaSSampleWebApp.Services;

    [Authorize]
    public class CommonController : Controller
    {
        private readonly ITokenAcquisition _tokenAcquisition;
        private readonly Iconfiguration _saaSWebApiOptions;
        private readonly IMSGraphHelper _msGraphHelper;

        public CommonController(
            IOptions<Iconfiguration> saaSWebApiOptions,
            ITokenAcquisition tokenAcquisition,
            IMSGraphHelper msGraphHelper)
        {
            _tokenAcquisition = tokenAcquisition;
            _saaSWebApiOptions = saaSWebApiOptions.Value;
            _msGraphHelper = msGraphHelper;
        }

        [HttpGet]
        [AuthorizeForScopes(ScopeKeySection = "SaaSWebApi:Scopes")]
        public async Task<IActionResult> GetSaaSWebApiAccessToken()
        {
            return Ok(await _tokenAcquisition
                .GetAccessTokenOnBehalfOfUserAsync(_saaSWebApiOptions.Scopes.Split(' ')));
        }

        [HttpGet]
        public async Task<IActionResult> GetTenantInfo()
        {
            var organization = await _msGraphHelper.GetOrganizationAsync();

            return Ok(new
            {
                tenantId = organization.Id,
                tenantName = organization.DisplayName
            });
        }

        [HttpGet]
        public async Task<IActionResult> GetUserByEmail([FromQuery] string email)
        {
            var user = await _msGraphHelper.GetUserAsync(email);

            if (user != null)
            {
                return Ok(new
                {
                    id = user.Id,
                    mail = user.UserPrincipalName
                });
            }

            return NotFound();
        }


        [HttpGet]
        public async Task<IActionResult> GetMailById([FromQuery] Guid userId)
        {
            var user = await _msGraphHelper.GetUserAsync(userId);

            if (user != null)
            {
                return Ok(new
                {
                    id = user.Id,
                    mail = user.Mail
                });
            }

            return NotFound();
        }
    }
}