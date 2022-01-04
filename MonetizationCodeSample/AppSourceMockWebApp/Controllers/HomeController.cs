// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace AppSourceMockWebApp.Controllers
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;

    using AppSourceMockWebApp.DAL;
    using AppSourceMockWebApp.Models;
    using AppSourceMockWebApp.Service;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.Extensions.Caching.Memory;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Identity.Web;
    using Newtonsoft.Json;

    [Authorize]
    public class HomeController : Controller
    {
        private readonly IConfiguration _configuration;
        private readonly IMemoryCache _cache;
        private readonly IWebhookTriggerService _webhookTriggerService;
        private readonly MemoryCacheEntryOptions _cacheEntryOptions =
            new MemoryCacheEntryOptions().SetSlidingExpiration(TimeSpan.FromMinutes(60));
        private readonly SubscriptionDbContext _dbContext;

        public HomeController(
            IConfiguration configuration,
            IWebhookTriggerService webhookTriggerService,
            IMemoryCache memoryCache,
            SubscriptionDbContext subscriptionDbContext
            )
        {
            _configuration = configuration;
            _cache = memoryCache;
            _dbContext = subscriptionDbContext;
            _webhookTriggerService = webhookTriggerService;
        }

        /// <summary>
        /// app list page
        /// </summary>
        public IActionResult Index()
        {
            var tenantId = Guid.Parse(User.GetTenantId());

            var subscription = _dbContext.Subscriptions.Where(i => i.Beneficiary == tenantId).SingleOrDefault();

            if (subscription != null)
            {
                return RedirectToAction(nameof(UpdateSubscription), new { subscriptionId = subscription.Id });
            }

            ViewBag.apps = new List<SaaSOfferMockData.Offer> {
                SaaSOfferMockData.Offers.ContosoAppOffer
            };

            return View();
        }

        /// <summary>
        /// app purchasement page
        /// </summary>
        public IActionResult App(string OfferID)
        {
            if (SaaSOfferMockData.Offers.ContosoAppOffer.OfferID == OfferID)
            {
                var appModel = new AppViewModel
                {
                    AppOffer = SaaSOfferMockData.Offers.ContosoAppOffer,
                    Plan = MockDataService.Plans.First()
                };

                return View(appModel);
            }

            return NotFound();
        }

        /// <summary>
        /// subscription editor page
        /// </summary>
        public IActionResult UpdateSubscription(Guid subscriptionId)
        {
            var viewModel = new UpdateSubscriptionViewModel()
            {
                Subscription = _dbContext.Subscriptions.Where(
                    subscription => subscription.Id == subscriptionId).Single()
            };

            return View(viewModel);
        }

        [HttpPost]
        public async Task<IActionResult> UpdateSubscription(UpdateSubscriptionViewModel model)
        {
            var subscription = _dbContext.Subscriptions.Where(
                subscription => subscription.Id == model.Subscription.Id).Single();

            OperationUpdate operation = null;

            // check if the plan is changed
            if (model.Subscription.PlanId != subscription.PlanId)
            {
                // create change plan operation 
                operation = new OperationUpdate()
                {
                    Id = Guid.NewGuid(),
                    Action = SaaSActionEnum.ChangePlan,
                    SubscriptionId = subscription.Id,
                    Quantity = model.Subscription.Quantity,
                    Status = SaasStatusEnum.InProgress,
                    PlanId = model.Subscription.PlanId
                };

                model.OperationUpdateId = operation.Id;

                // save data in cache
                _cache.Set(operation.Id, operation, _cacheEntryOptions);
                // webhook trigger: send notifaction to ISV service
                await _webhookTriggerService.NotifyAsync(operation);
            }
 
            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> CancelSubscription(UpdateSubscriptionViewModel model)
        {
            var subscription = _dbContext.Subscriptions.Where(i => i.Id == model.Subscription.Id).Single();

            // create cancel subscription operation  
            var operation = new OperationUpdate()
            {
                Id = Guid.NewGuid(),
                Action = SaaSActionEnum.Unsubscribe,
                SubscriptionId = subscription.Id,
                Quantity = model.Subscription.Quantity,
                Status = SaasStatusEnum.InProgress,
                PlanId = model.Subscription.PlanId
            };
            model.OperationUpdateId = operation.Id;

            // save data in cache
            _cache.Set(operation.Id, operation, _cacheEntryOptions);
            // webhook trigger: send notifaction to ISV service
            await _webhookTriggerService.NotifyAsync(operation);

            return View("UpdateSubscription", model);
        }

        [HttpPost]
        public IActionResult App(AppViewModel model)
        {
            var resolvedSubscription = new ResolvedSubscription()
            {
                SubscriptionId = Guid.NewGuid(),
                SubscriptionName = $"{model.AppOffer.Name} subscription",
                OfferId = model.AppOffer.OfferID,
                PlanId = model.Plan.PlanId,
                Quantity = model.Quantity,
                TenantId = Guid.Parse(User.GetTenantId())
            };

            // save data in cache
            _cache.Set(resolvedSubscription.SubscriptionId, resolvedSubscription, _cacheEntryOptions);

            // generate fake token
            var json = JsonConvert.SerializeObject(resolvedSubscription);
            var plainTextBytes = Encoding.UTF8.GetBytes(json);
            var base64String = Convert.ToBase64String(plainTextBytes);
            string redirectUrl = _configuration["SaaSOffer:LandingpageURL"] + "?token=" + base64String;

            return Redirect(redirectUrl);
        }

        [AllowAnonymous]
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
