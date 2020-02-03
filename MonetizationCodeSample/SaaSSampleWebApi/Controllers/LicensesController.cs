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
    /// licenses api controller
    /// </summary>
    [Authorize]
    [ApiController]
    [Route("api/Subscriptions/{subscriptionId}/[controller]")]
    public class LicensesController : ControllerBase
    {
        private readonly LicenseDbContext _licenseDbContext;

        public LicensesController(LicenseDbContext licenseDbContext)
        {
            _licenseDbContext = licenseDbContext;
        }

        /// <summary>
        /// get all licenses under the specific subscription
        /// </summary>
        [HttpGet]
        public IActionResult Get(Guid subscriptionId)
        {
            var subscription = _licenseDbContext.Subscriptions.Where(subscription => subscription.Id == subscriptionId).SingleOrDefault();

            if (subscription == null) return NotFound();

            return Ok(_licenseDbContext.Licences.Where(licence => licence.SubscriptionId == subscription.Id));
        }

        /// <summary>
        /// get license details by id
        /// </summary>
        [Route("{id}")]
        [HttpGet]
        public IActionResult GetById(Guid id)
        {
            var licence = _licenseDbContext.Licences.Where(licence => licence.Id == id).SingleOrDefault();

            if (licence == null) return NotFound();

            return Ok(licence);
        }

        /// <summary>
        /// create a new license
        /// </summary>
        [HttpPost]
        public IActionResult Post(Guid subscriptionId, Licence payload)
        {
            var subscription = _licenseDbContext.Subscriptions.Where(subscription => subscription.Id == subscriptionId).SingleOrDefault();

            if (subscription == null) return BadRequest();

            var license = new Licence()
            {
                Id = Guid.NewGuid(),
                SubscriptionId = subscriptionId,
                UserEmail = payload.UserEmail,
                UserId = payload.UserId
            };

            _licenseDbContext.Licences.Add(license);

            _licenseDbContext.SaveChanges();

            return CreatedAtAction(nameof(GetById), new { subscriptionId, id  = license.Id }, license);
        }

        /// <summary>
        /// delete a license by id
        /// </summary>
        [HttpDelete("{id}")]
        public IActionResult Delete(Guid id)
        {
            var license = _licenseDbContext.Licences.Where(license => license.Id == id).SingleOrDefault();

            if (license != null)
            {
                _licenseDbContext.Licences.Remove(license);
                _licenseDbContext.SaveChanges();

                return NoContent();
            }

            return NotFound();
        }
    }
}