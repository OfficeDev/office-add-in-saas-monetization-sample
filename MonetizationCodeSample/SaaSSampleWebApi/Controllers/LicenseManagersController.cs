// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace SaaSSampleWebApi.Controllers
{
    using System;
    using System.Linq;

    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Mvc;
    using SaaSSampleWebApi.DAl;
    using SaaSSampleWebApi.Models;

    /// <summary>
    /// license managers api controller
    /// </summary>
    [Authorize]
    [ApiController]
    [Route("api")]
    public class LicenseManagersController : ControllerBase
    {
        private readonly LicenseDbContext _licenseDbContext;

        public LicenseManagersController(LicenseDbContext licenseDbContext)
        {
            _licenseDbContext = licenseDbContext;
        }

        /// <summary>
        /// get all license managers under the specific subscription
        /// </summary>
        [Route("Subscriptions/{subscriptionId}/LicenseManagers")]
        [HttpGet]
        public IActionResult Get(Guid subscriptionId)
        {
            var subscription = _licenseDbContext.Subscriptions.Where(subscription => subscription.Id == subscriptionId).SingleOrDefault();

            if (subscription == null) return NotFound();

            return Ok(_licenseDbContext.LicenseManagers.Where(user => user.SubscriptionId == subscription.Id));
        }

        /// <summary>
        /// get license manager details by Id
        /// </summary>
        [Route("Subscriptions/{subscriptionId}/LicenseManagers/{userId}")]
        [HttpGet]
        public IActionResult GetById(Guid subscriptionId, Guid userId)
        {
            var manager = _licenseDbContext.LicenseManagers.Where(user => user.SubscriptionId == subscriptionId && user.UserId == userId ).SingleOrDefault();

            if (manager == null) return NotFound();

            return Ok(manager);
        }


        /// <summary>
        /// create a new license manager
        /// </summary>
        [Route("Subscriptions/{subscriptionId}/LicenseManagers")]
        [HttpPost]
        public IActionResult Post(Guid subscriptionId, [FromBody] LicenseManager payload)
        {
            if (payload == null) return BadRequest();

            var subscription = _licenseDbContext.Subscriptions.Where(subscription => subscription.Id == subscriptionId).SingleOrDefault();

            if (subscription == null) return BadRequest();

            var manager = new LicenseManager()
            {
                Id = Guid.NewGuid(),
                SubscriptionId = subscription.Id,
                UserId = payload.UserId,
                UserEmail = payload.UserEmail,
                IsAdmin = payload.IsAdmin
            };

            _licenseDbContext.LicenseManagers.Add(manager);

            _licenseDbContext.SaveChanges();

            return CreatedAtAction(nameof(GetById), new { subscriptionId, userId = manager.UserId }, manager);
        }

        /// <summary>
        /// delete a license manager by id
        /// </summary>
        [Route("Subscriptions/{subscriptionId}/LicenseManagers/{userId}")]
        [HttpDelete]
        public IActionResult Delete(Guid subscriptionId, Guid userId)
        {
            var manager = _licenseDbContext.LicenseManagers.Where(user => user.SubscriptionId == subscriptionId && user.UserId == userId).SingleOrDefault();

            if (manager != null)
            {
                _licenseDbContext.LicenseManagers.Remove(manager);
                _licenseDbContext.SaveChanges();

                return NoContent();
            }

            return NotFound();
        }
    }
}