namespace ExcelAddInWeb.Controllers
{
    using System;
    using System.Threading.Tasks;

    using Microsoft.AspNetCore.Mvc;
    using Microsoft.Extensions.Options;
    using Microsoft.Identity.Client;
    using Newtonsoft.Json;
    using ExcelAddInWeb.Models;
    using ExcelAddInWeb.Services;
    using SaaSOfferMockData;

    public class AzureADAuthController : Controller
    {
        private readonly AzureAdOptions _azureAdOptions;
        private readonly IAssignedUserService _assignedUserService;

        Uri LoginRedirectUri => new Uri(Url.Action(nameof(Authorize), "AzureADAuth", null, Request.Scheme));

        public AzureADAuthController(IOptions<AzureAdOptions> azureAdOptions, IAssignedUserService assignedUserService)
        {
            _azureAdOptions = azureAdOptions.Value;
            _assignedUserService = assignedUserService;
        }

        public async Task<ActionResult> Login()
        {
            if (string.IsNullOrEmpty(_azureAdOptions.ClientId) || string.IsNullOrEmpty(_azureAdOptions.ClientSecret))
            {
                ViewBag.Message = "Please set your client ID and client secret in the Web.config file";
                return View();
            }

            ConfidentialClientApplicationBuilder clientBuilder = ConfidentialClientApplicationBuilder.Create(_azureAdOptions.ClientId);
            ConfidentialClientApplication clientApp = (ConfidentialClientApplication)clientBuilder.Build();

            string[] graphScopes = { "profile" };
            var urlBuilder = clientApp.GetAuthorizationRequestUrl(graphScopes);
            urlBuilder.WithRedirectUri(LoginRedirectUri.ToString());
            urlBuilder.WithAuthority(_azureAdOptions.Authority);


            var authUrl = await urlBuilder.ExecuteAsync(System.Threading.CancellationToken.None);
            return Redirect(authUrl.ToString());
        }
        public async Task<ActionResult> Authorize()
        {

            ConfidentialClientApplicationBuilder clientBuilder = ConfidentialClientApplicationBuilder.Create(_azureAdOptions.ClientId);
            clientBuilder.WithClientSecret(_azureAdOptions.ClientSecret);
            clientBuilder.WithRedirectUri(LoginRedirectUri.ToString());
            clientBuilder.WithAuthority(_azureAdOptions.Authority);

            ConfidentialClientApplication clientApp = (ConfidentialClientApplication)clientBuilder.Build();
            string[] sassScopes = $"{_azureAdOptions.SaaSScopes}".Split(new[] { ' ' });

            var authResultBuilder = clientApp.AcquireTokenByAuthorizationCode(
                sassScopes,
                HttpContext.Request.Query["code"].ToString()
            );
            try
            {
                var authResult = await authResultBuilder.ExecuteAsync();
                var offer = Offers.ContosoAppOffer;
                var activation = await _assignedUserService.Activate($"{_azureAdOptions.SaaSAPI}/{offer.OfferID}", authResult.AccessToken);

                ViewBag.Message = JsonConvert.SerializeObject(
                    new { status = "success", activation = activation, accountName = authResult.Account.Username });
            }
            catch (Exception e)
            {
                ViewBag.Message = JsonConvert.SerializeObject(new { status = "failure", error = e.Message });
            }

            return View();
        }
    }
}