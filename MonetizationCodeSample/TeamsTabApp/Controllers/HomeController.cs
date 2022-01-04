// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace TeamsTabApp.Controllers
{
    using System.Diagnostics;
    using System.Threading.Tasks;

    using Microsoft.AspNetCore.Mvc;
    using TeamsTabApp.Auth;
    using TeamsTabApp.Models;
    using TeamsTabApp.Services;
    using SaaSOfferMockData;
    using Microsoft.Extensions.Options;

    public class HomeController : Controller
    {
        private readonly IAssignedUserService _assignedUserService;
        private readonly ITokenService _tokenService;
        private readonly AzureAdOptions _azureAdOptions;

        public HomeController(IOptions<AzureAdOptions> azureAdOptions, IAssignedUserService assignedUserService, ITokenService tokenService)
        {
            _azureAdOptions = azureAdOptions.Value;
            _assignedUserService = assignedUserService;
            _tokenService = tokenService;
        }

        public IActionResult Index()
        {
            return View();
        }

        public async Task<IActionResult> ValidateLicense()
        {
            var accessToken = _tokenService.GetAccessToken();
            var offer = Offers.ContosoAppOffer;
            var activation = await _assignedUserService.Activate($"{_azureAdOptions.SaaSAPI}/{offer.OfferID}", await accessToken);
            return new JsonResult(activation);
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
