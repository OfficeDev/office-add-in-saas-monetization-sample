// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace SaaSSampleWebApp.Services
{
    using System;
    using System.Threading.Tasks;

    using SaaSSampleWebApp.Models;

    public interface ISubscriptionService
    {
        Task<Subscription> GetSubscriptionAsync(Guid tenantId);

        Task<LicenseManager> CheckLicenseManagerAsync(Guid subscriptionId, Guid userId);
    }
}
