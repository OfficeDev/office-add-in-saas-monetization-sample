// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace AppSourceMockWebApp.Controllers
{
    using System;
    using System.Linq;
    using System.Text;

    using AppSourceMockWebApp.DAL;
    using AppSourceMockWebApp.Models;
    using AppSourceMockWebApp.Service;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.Extensions.Caching.Memory;
    using Newtonsoft.Json;

    [AllowAnonymous]
    [Route("api/saas")]
    [ApiController]
    public class SaaSFulfillmentAPIsController : ControllerBase
    {
        private readonly IMemoryCache _cache;
        private readonly IWebhookTriggerService _webhookTriggerService;
        private readonly MemoryCacheEntryOptions _cacheEntryOptions = 
            new MemoryCacheEntryOptions().SetSlidingExpiration(TimeSpan.FromMinutes(60));
        private readonly SubscriptionDbContext _dbContext;

        public SaaSFulfillmentAPIsController(
            IMemoryCache memoryCache, 
            SubscriptionDbContext subscriptionDbContext,
            IWebhookTriggerService webhookTriggerService)
        {
            _cache = memoryCache;
            _dbContext = subscriptionDbContext;
            _webhookTriggerService = webhookTriggerService;
        }

        // Resolve a purchased subscription
        // https://docs.microsoft.com/en-us/azure/marketplace/partner-center-portal/pc-saas-fulfillment-api-v2#resolve-a-purchased-subscription
        [Route("subscriptions/resolve")]
        [HttpPost]
        public IActionResult ResolveSubscription()
        {
            var token = this.Request.Headers["x-ms-marketplace-token"];
            byte[] data = Convert.FromBase64String(token);
            string decodedString = Encoding.UTF8.GetString(data);
            var resolvedSubscription = JsonConvert.DeserializeObject<ResolvedSubscription>(decodedString);
            return Ok(resolvedSubscription);
        }

        // Activate a subscription
        // https://docs.microsoft.com/en-us/azure/marketplace/partner-center-portal/pc-saas-fulfillment-api-v2#activate-a-subscription
        [Route("subscriptions/{subscriptionId}/activate")]
        [HttpPost]
        public IActionResult ActivateSubscription([FromRoute]Guid subscriptionId, [FromBody] ActivatePayload activatePayload)
        {
            if (_cache.TryGetValue(subscriptionId, out ResolvedSubscription cacheEntry))
            {
                var resolvedSubscription = cacheEntry as ResolvedSubscription;

                if (activatePayload.PlanId == resolvedSubscription.PlanId
                    && activatePayload.Quantity == resolvedSubscription.Quantity)
                {
                    var subscription = new Subscription
                    {
                        Id = resolvedSubscription.SubscriptionId,
                        Name = resolvedSubscription.SubscriptionName,
                        PlanId = resolvedSubscription.PlanId,
                        Quantity = resolvedSubscription.Quantity,
                        Beneficiary = resolvedSubscription.TenantId,
                        OfferId = resolvedSubscription.OfferId
                    };

                    _dbContext.Subscriptions.Add(subscription);

                    _dbContext.SaveChanges();

                    return Ok();
                }
            }

            return BadRequest();
        }

        // Get operation status
        // https://docs.microsoft.com/en-us/azure/marketplace/partner-center-portal/pc-saas-fulfillment-api-v2#get-operation-status
        [Route("subscriptions/{subscriptionId}/operations/{operationId}")]
        [HttpGet]
        public IActionResult GetOperation([FromRoute]Guid subscriptionId, [FromRoute]Guid operationId)
        {
            if (_cache.TryGetValue(operationId, out OperationUpdate cacheEntry))
            {
                return Ok(cacheEntry);
            }

            return BadRequest();
        }

        // Update the status of an operation
        // https://docs.microsoft.com/en-us/azure/marketplace/partner-center-portal/pc-saas-fulfillment-api-v2#update-the-status-of-an-operation
        [Route("subscriptions/{subscriptionId}/operations/{operationId}")]
        [HttpPatch]
        public IActionResult PatchOperation([FromRoute]Guid subscriptionId, [FromRoute]Guid operationId)
        {
            var subscription = _dbContext.Subscriptions.Where(subscription => subscription.Id == subscriptionId).Single();

            if (_cache.TryGetValue(operationId, out OperationUpdate cacheEntry))
            {
                if (cacheEntry.Action == SaaSActionEnum.Unsubscribe)
                {
                    _dbContext.Subscriptions.Remove(subscription);
                }
                else
                {
                    subscription.PlanId = cacheEntry.PlanId;
                    subscription.Quantity = cacheEntry.Quantity;
                    _dbContext.Subscriptions.Update(subscription);
                }

                _dbContext.SaveChanges();

                // update data in cache.
                cacheEntry.Status = SaasStatusEnum.Succeeded;
                _cache.Set(cacheEntry.Id, cacheEntry, _cacheEntryOptions);

                return Ok();
            }

            return BadRequest();
        }

        // Cancel a subscription
        // https://docs.microsoft.com/en-us/azure/marketplace/partner-center-portal/pc-saas-fulfillment-api-v2#cancel-a-subscription
        [Route("subscriptions/{subscriptionId}")]
        [HttpDelete]
        public IActionResult DeleteSubscription([FromRoute]Guid subscriptionId)
        {
            // create cancel subscription operation  
            var operation = new OperationUpdate()
            {
                Id = Guid.NewGuid(),
                Action = SaaSActionEnum.Unsubscribe,
                SubscriptionId = subscriptionId,
                Status = SaasStatusEnum.InProgress
            };

            // save data in cache.
            _cache.Set(operation.Id, operation, _cacheEntryOptions);

            _webhookTriggerService.NotifyAsync(operation);

            return StatusCode(202);
        }
    }
}