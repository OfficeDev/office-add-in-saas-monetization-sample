// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace SaaSSampleWebApi.Controllers
{
    using System;
    using System.Linq;
    using System.Net.Http;
    using System.Text;
    using System.Threading.Tasks;

    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.Logging;
    using Newtonsoft.Json;
    using SaaSSampleWebApi.DAl;
    using SaaSSampleWebApi.Models;

    /// <summary>
    /// webhook handler
    /// </summary>
    [ApiController]
    [AllowAnonymous]
    [RequireHttps]
    [Route("api/[controller]")]
    public class WebHookController : ControllerBase
    {
        private readonly IHttpClientFactory _httpClientFactory;

        private readonly IConfiguration _configuration;

        private readonly ILogger<WebHookController> _logger;

        private readonly LicenseDbContext _licenseDbContext;

        public WebHookController(
            IConfiguration configuration,
            IHttpClientFactory httpClientFactory,
            ILogger<WebHookController> logger,
            LicenseDbContext licenseDbContext)
        {
            _httpClientFactory = httpClientFactory;
            _configuration = configuration;
            _licenseDbContext = licenseDbContext;
            _logger = logger;
        }

        // In a webhook notification, actionable statuses are either Succeeded and Failed. 
        // An operation's life cycle is from NotStarted to a terminal state like Succeeded, Failed, or Conflict. 
        // If you receive NotStarted or InProgress, continue to request the status via GET API until the operation reaches a terminal state before taking action.
        // Please refer to https://docs.microsoft.com/en-us/azure/marketplace/partner-center-portal/pc-saas-fulfillment-api-v2#implementing-a-webhook-on-the-saas-service to learn more details.
        [HttpPost]
        public async Task<IActionResult> Post(WebhookPayload payload)
        {
            _logger.LogInformation($"Received webhook request: {JsonConvert.SerializeObject(payload)}");

            var subscription = _licenseDbContext.Subscriptions.Where(subscription => subscription.Id == payload.SubscriptionId).Single();
            switch (payload.Action)
            {
                case "ChangePlan":
                    if (!payload.Quantity.HasValue)
                    {
                        var licences = _licenseDbContext.Licences.Where(licence => licence.SubscriptionId == subscription.Id);

                        foreach (var licence in licences)
                        {
                            _licenseDbContext.Licences.Remove(licence);
                        }
                    }
                    subscription.PlanId = payload.PlanId;
                    subscription.PurchaseSeatsCount = payload.Quantity;
                    _licenseDbContext.Subscriptions.Update(subscription);
                    await Patch(new OperationPayload()
                    {
                        PlanId = payload.PlanId,
                        Quantity = payload.Quantity.HasValue ? payload.Quantity.ToString() : string.Empty,
                        Status = "Success"
                    }, payload.SubscriptionId, payload.Id).ConfigureAwait(false);
                    break;
                case "ChangeQuantity":
                    subscription.PurchaseSeatsCount = payload.Quantity;
                    _licenseDbContext.Subscriptions.Update(subscription);
                    await Patch(new OperationPayload()
                    {
                        PlanId = payload.PlanId,
                        Quantity = payload.Quantity.Value.ToString(),
                        Status = "Success"
                    }, payload.SubscriptionId, payload.Id).ConfigureAwait(false);
                    break;
                case "Unsubscribe":
                    var managers = _licenseDbContext.LicenseManagers.Where(manager => manager.SubscriptionId == subscription.Id);
                    _licenseDbContext.LicenseManagers.RemoveRange(managers);

                    var licenses = _licenseDbContext.Licences.Where(license => license.SubscriptionId == subscription.Id);
                    _licenseDbContext.Licences.RemoveRange(licenses);

                    _licenseDbContext.Subscriptions.Remove(subscription);
                    await Patch(new OperationPayload()
                    {
                        Status = "Success"
                    }, payload.SubscriptionId, payload.Id).ConfigureAwait(false);
                    break;
                default:
                    throw new Exception();
            }

            _licenseDbContext.SaveChanges();

            return Ok();
        }

        private async Task Patch(OperationPayload payload, Guid subscriptionId, Guid operationId)
        {
            using var requestMessage = new HttpRequestMessage(HttpMethod.Patch,
                    $"{_configuration["SaaSfulfillmentAPIs:ApiEndPoint"]}/api/saas/subscriptions/{subscriptionId}/operations/{operationId}?api-version={_configuration["SaaSfulfillmentAPIs:ApiVersion"]}");

            // the token is not required for Mock APIs 
            // requestMessage.Headers.Authorization = new AuthenticationHeaderValue("Bearer", your_token);

            var json = JsonConvert.SerializeObject(payload);

            using var stringContent = new StringContent(json, Encoding.UTF8, "application/json");
            requestMessage.Content = stringContent;
            var httpClient = _httpClientFactory.CreateClient();

            using var response = await httpClient.SendAsync(requestMessage).ConfigureAwait(false);
            response.EnsureSuccessStatusCode();
        }
    }
}