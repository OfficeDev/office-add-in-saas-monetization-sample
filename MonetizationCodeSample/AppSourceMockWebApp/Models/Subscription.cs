// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace AppSourceMockWebApp.Models
{
    using System;
    using System.ComponentModel.DataAnnotations;

    public class Subscription
    {
        [Key]
        // The GUID identifier for a SaaS resource.
        public Guid Id { get; set; }

        public string Name { get; set; }

        // A unique string identifier for each offer.
        public string OfferId { get; set; }

        // Tenant, object id and email address for which SaaS subscription is purchased.
        public Guid Beneficiary { get; set; }

        // A unique string identifier for each plan/SKU
        public string PlanId { get; set; }

        public int? Quantity { get; set; }
    }
}
