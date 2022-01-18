// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace SaaSSampleWebApi.Controllers
{
    using System;
    using System.Linq;
    using System.Net;
    using System.Net.Http;
    using System.Text;
    using System.Threading.Tasks;

    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Identity.Web;
    using Newtonsoft.Json;
    using SaaSSampleWebApi.DAl;
    using SaaSSampleWebApi.Models;

    /// <summary>
    /// subscriptions api controller
    /// </summary>
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class SubscriptionsController : ControllerBase
    {
        private readonly IConfiguration _configuration;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly LicenseDbContext _licenseDbContext;

        public SubscriptionsController(
            IHttpClientFactory httpClientFactory,
            IConfiguration configuration,
            LicenseDbContext licenseDbContext)
        {
            _configuration = configuration;
            _httpClientFactory = httpClientFactory;
            _licenseDbContext = licenseDbContext;
        }

        /// <summary>
        /// get all subscriptions
        /// </summary>
        [HttpGet]
        public IActionResult Get()
        {
            return Ok(_licenseDbContext.Subscriptions);
        }

        /// <summary>
        /// get subscription details by id
        /// </summary>
        [HttpGet("{id}")]
        public IActionResult GetById(Guid id)
        {
            var findSubscription = _licenseDbContext.Subscriptions.Where(subscription => subscription.Id == id).SingleOrDefault();

            if (findSubscription == null) return NotFound();

            return Ok(findSubscription);
        }

        /// <summary>
        /// create a new subscription
        /// </summary>
        [HttpPost]
        public IActionResult Post(Subscription payload)
        {
            if (payload == null) return BadRequest();

            _licenseDbContext.Subscriptions.Add(payload);

            _licenseDbContext.SaveChanges();

            return CreatedAtAction(nameof(GetById), new { id = payload.Id }, payload);
        }

        /// <summary>
        /// update a specific subscription
        /// </summary>
        [HttpPut("{subscriptionId}")]
        public IActionResult Put(Guid subscriptionId, Subscription payload)
        {
            if (payload == null) return BadRequest();

            var subscription = _licenseDbContext.Subscriptions.Where(subscription => subscription.Id == subscriptionId).SingleOrDefault();

            if (subscription == null) return BadRequest();

            subscription.AllowOverAssignment = payload.AllowOverAssignment;
            subscription.FirstComeFirstServedAssignment = payload.FirstComeFirstServedAssignment;

            _licenseDbContext.Subscriptions.Update(subscription);

            _licenseDbContext.SaveChanges();

            return Ok(subscription);
        }

        /// <summary>
        /// delete a subscription by id
        /// </summary>
        [HttpDelete("{subscriptionId}")]
        public async Task<IActionResult> CancelSubscriptionAsync(Guid subscriptionId)
        {
            var subscription = _licenseDbContext.Subscriptions.Where(subscription => subscription.Id == subscriptionId).SingleOrDefault();

            if (subscription == null) return BadRequest();

            using var requestMessage = new HttpRequestMessage(HttpMethod.Delete,
                $"{_configuration["SaaSfulfillmentAPIs:ApiEndPoint"]}/api/saas/subscriptions/{subscription.Id}?api-version={_configuration["SaaSfulfillmentAPIs:ApiVersion"]}");

            // the token is not required for Mock APIs 
            // requestMessage.Headers.Authorization = new AuthenticationHeaderValue("Bearer", your_token);

            var httpClient = _httpClientFactory.CreateClient();

            using (var response = await httpClient.SendAsync(requestMessage).ConfigureAwait(false))
            {
                if (response.StatusCode == HttpStatusCode.Accepted)
                {
                    return Accepted();
                }

                return BadRequest(response.ReasonPhrase);
            }
        }

        /// <summary>
        /// resolve auth code
        /// </summary>
        [Route("resolve")]
        [HttpPost]
        public async Task<IActionResult> ResolveAsync([FromForm] string AuthCode)
        {
            using var requestMessage = new HttpRequestMessage(HttpMethod.Post,
                $"{_configuration["SaaSfulfillmentAPIs:ApiEndPoint"]}/api/saas/subscriptions/resolve?api-version={_configuration["SaaSfulfillmentAPIs:ApiVersion"]}");

            // the token is not required for Mock APIs 
            // requestMessage.Headers.Authorization = new AuthenticationHeaderValue("Bearer", your_token);

            requestMessage.Headers.Add("x-ms-marketplace-token", AuthCode);

            var httpClient = _httpClientFactory.CreateClient();

            using (var response = await httpClient.SendAsync(requestMessage).ConfigureAwait(false))
            {
                if (response.StatusCode == HttpStatusCode.OK)
                {
                    var content = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
                    var resolvedSubscription = JsonConvert.DeserializeObject<ResolvedSubscription>(content);

                    var subscription = new Subscription
                    {
                        Id = resolvedSubscription.Id,
                        OfferId = resolvedSubscription.OfferId,
                        PlanId = resolvedSubscription.PlanId,
                        SubscriptionName = resolvedSubscription.SubscriptionName,
                        Purchaser = HttpContext.User.Identity.Name,
                        PurchaserId = Guid.Parse(HttpContext.User.GetObjectId()),
                        TenantId = Guid.Parse(HttpContext.User.GetTenantId()),
                        PurchaseSeatsCount = resolvedSubscription.Quantity,
                        AllowOverAssignment = false,
                        FirstComeFirstServedAssignment = false
                    };
                    return Ok(subscription);
                }
                return BadRequest(response.ReasonPhrase);
            }
        }

        /// <summary>
        /// activate the specific subscription
        /// </summary>
        [Route("{subscriptionId}/activate")]
        [HttpPost]
        public async Task<IActionResult> ActivateAsync(Guid subscriptionId)
        {
            var tenantId = Guid.Parse(HttpContext.User.GetTenantId());

            var subscription = _licenseDbContext.Subscriptions.Where(subscription => subscription.Id == subscriptionId).Single();

            using var requestMessage = new HttpRequestMessage(HttpMethod.Post,
                $"{_configuration["SaaSfulfillmentAPIs:ApiEndPoint"]}/api/saas/subscriptions/{subscription.Id}/activate?api-version={_configuration["SaaSfulfillmentAPIs:ApiVersion"]}");

            // the token is not required for Mock APIs 
            // requestMessage.Headers.Authorization = new AuthenticationHeaderValue("Bearer", your_token);

            var activatePayload = new SubscriptionPayload()
            {
                PlanId = subscription.PlanId,
                Quantity = subscription.PurchaseSeatsCount
            };
            var json = JsonConvert.SerializeObject(activatePayload);
            using var stringContent = new StringContent(json, Encoding.UTF8, "application/json");
            requestMessage.Content = stringContent;

            var httpClient = _httpClientFactory.CreateClient();
            using var response = await httpClient.SendAsync(requestMessage).ConfigureAwait(false);
            if (response.StatusCode == HttpStatusCode.OK)
            {
                return Ok();
            }

            return BadRequest(response.ReasonPhrase);
        }

        /// <summary>
        /// check if there is a available license for the app and return license status
        /// </summary>
        [HttpPost]
        [Route("CheckOrActivateLicense/{offerId}")]
        public IActionResult CheckOrActivateLicense(string offerId)
        {
            var statusResult = new LicenseStatus();

            var tenantId = Guid.Parse(HttpContext.User.GetTenantId());

            var userId = Guid.Parse(HttpContext.User.GetObjectId());

            var subscription = _licenseDbContext.Subscriptions.Where(subscription =>
                subscription.OfferId == offerId && subscription.TenantId == tenantId).FirstOrDefault();

            if (subscription == null)
            {
                statusResult.Reason = "Tenant has not purchased a license";
                statusResult.Status = ActivationStatusEnum.Failure;

                return Ok(statusResult);
            }

            statusResult.FirstComeFirstServedAssignment = subscription.FirstComeFirstServedAssignment;
            statusResult.AllowOverAssignment = subscription.AllowOverAssignment;
            statusResult.LicenseAmount = subscription.PurchaseSeatsCount;
            statusResult.AvailableLicenseQuantity = subscription.LicenceType == LicenceType.SeatBased ?
                subscription.PurchaseSeatsCount - _licenseDbContext.Licences.Count(licence => licence.SubscriptionId == subscription.Id) : null;

            if (subscription.LicenceType == LicenceType.SiteBased)
            {
                statusResult.Reason = "Tenant has purchased a site license.";
                statusResult.Status = ActivationStatusEnum.Success;

                return Ok(statusResult);
            }

            if (_licenseDbContext.Licences.Where(licence => licence.UserId == userId && licence.SubscriptionId == subscription.Id).FirstOrDefault() != null)
            {
                statusResult.Reason = "Tenant has purchased licenses and user has a license assigned";
                statusResult.Status = ActivationStatusEnum.Success;

                var assigedUser = _licenseDbContext.Licences.Where(licence => licence.UserId == userId && licence.SubscriptionId == subscription.Id).First();
                assigedUser.ActivationDateTime = DateTime.Now;
                _licenseDbContext.Licences.Update(assigedUser);
                _licenseDbContext.SaveChanges();

                return Ok(statusResult);
            }

            if (statusResult.AvailableLicenseQuantity > 0 && subscription.FirstComeFirstServedAssignment)
            {
                statusResult.Reason = "Tenant has purchased licenses. User did not initially have a license, but just had an available license auto-assigned to them";
                statusResult.Status = ActivationStatusEnum.Success;

                _licenseDbContext.Licences.Add(new Licence
                {
                    Id = Guid.NewGuid(),
                    SubscriptionId = subscription.Id,
                    UserId = userId,
                    UserEmail = User.Identity.Name,
                    ActivationDateTime = DateTime.Now
                });
                _licenseDbContext.SaveChanges();

                return Ok(statusResult);
            }

            if (subscription.AllowOverAssignment && statusResult.AvailableLicenseQuantity <= 0)
            {
                statusResult.Reason = "Tenant has purchased licenses but all licenses have been assigned. but AllowOverAssignment is enabled. ";
                statusResult.Status = ActivationStatusEnum.Success;

                _licenseDbContext.Licences.Add(new Licence
                {
                    Id = Guid.NewGuid(),
                    SubscriptionId = subscription.Id,
                    UserId = userId,
                    UserEmail = User.Identity.Name,
                    ActivationDateTime = DateTime.Now
                });
                _licenseDbContext.SaveChanges();

                return Ok(statusResult);
            }

            if (!statusResult.FirstComeFirstServedAssignment && statusResult.AvailableLicenseQuantity > 0)
            {
                statusResult.Reason = "Tenant has purchased licenses. User does not have a license, auto-assignment is not enabled. ";
                statusResult.Status = ActivationStatusEnum.Failure;

                return Ok(statusResult);
            }

            if (!statusResult.AllowOverAssignment == false && statusResult.AvailableLicenseQuantity == 0)
            {
                statusResult.Reason = "Tenant has purchased licenses. User does not have a license, license overrun is not enabled";
                statusResult.Status = ActivationStatusEnum.Failure;

                return Ok(statusResult);
            }

            return StatusCode(500);
        }
    }
}