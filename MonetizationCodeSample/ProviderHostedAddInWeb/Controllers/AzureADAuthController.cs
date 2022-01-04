using Microsoft.Identity.Client;
using System;
using System.Threading.Tasks;
using System.Web.Mvc;
using System.Configuration;
using ProviderHostedAddInWeb.Services;

namespace ProviderHostedAddInWeb.Controllers
{
    public class AzureADAuthController : Controller
    {
        string AzureADClientId = ConfigurationManager.AppSettings["AAD:ClientID"];
        string AzureADClientSecret = ConfigurationManager.AppSettings["AAD:ClientSecret"];
        string AzureADAuthority = ConfigurationManager.AppSettings["AAD:Authority"];
        string SaaSScopes = ConfigurationManager.AppSettings["SaaSScopes"];
        string SaaSAPI = ConfigurationManager.AppSettings["SaaSAPI"];
        string OfferID = "contoso_o365_addin";
        Uri loginRedirectUri => new Uri(Url.Action(nameof(Authorize), "AzureADAuth", null, Request.Url.Scheme));
        private IAssignedUserService _assignedUserService;
        public AzureADAuthController(IAssignedUserService assignedUserService)
        {
            _assignedUserService = assignedUserService;
        }

        /// <summary>
        /// Logs the user into Office 365.
        /// </summary>
        /// <param name="authState">The login or logout status of the user.</param>
        /// <returns>A redirect to the Office 365 login page.</returns>
        public async Task<ActionResult> Login()
        {
            if (string.IsNullOrEmpty(AzureADClientId) || string.IsNullOrEmpty(AzureADClientSecret))
            {
                ViewBag.Message = "Please set your client ID and client secret in the Web.config file";
                return View();
            }

            ConfidentialClientApplicationBuilder clientBuilder = ConfidentialClientApplicationBuilder.Create(AzureADClientId);
            ConfidentialClientApplication clientApp = (ConfidentialClientApplication)clientBuilder.Build();

            // Generate the parameterized URL for Azure login.
            string[] graphScopes = { "profile" };
            var urlBuilder = clientApp.GetAuthorizationRequestUrl(graphScopes);
            urlBuilder.WithRedirectUri(loginRedirectUri.ToString());
            urlBuilder.WithAuthority(AzureADAuthority);

            var authUrl = await urlBuilder.ExecuteAsync(System.Threading.CancellationToken.None);
            return Redirect(authUrl.ToString());
        }

        public async Task<ActionResult> Authorize()
        {

            ConfidentialClientApplicationBuilder clientBuilder = ConfidentialClientApplicationBuilder.Create(AzureADClientId);
            clientBuilder.WithClientSecret(AzureADClientSecret);
            clientBuilder.WithRedirectUri(loginRedirectUri.ToString());
            clientBuilder.WithAuthority(AzureADAuthority);

            ConfidentialClientApplication clientApp = (ConfidentialClientApplication)clientBuilder.Build();
            string[] sassScopes = $"{SaaSScopes}".Split(new[] { ' ' });
            try
            {
                // Get and save the token.
                var authResultBuilder = clientApp.AcquireTokenByAuthorizationCode(
                    sassScopes,
                    Request.Params["code"]
                );

                var authResult = await authResultBuilder.ExecuteAsync();
                var activation = await _assignedUserService.Activate($"{SaaSAPI}/{OfferID}", authResult.AccessToken);
                if (!activation.AvailableLicenseQuantity.HasValue)
                {
                    activation.AvailableLicenseQuantity = 0;
                }
                ViewBag.accountName = authResult.Account.Username;
                return View(activation);

            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.WriteLine(ex.ToString());
            }

            return View();
        }
    }
}