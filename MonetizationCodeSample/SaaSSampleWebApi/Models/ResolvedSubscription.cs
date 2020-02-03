// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace SaaSSampleWebApi.Models
{
    using System;

    public class ResolvedSubscription
    {
        public Guid Id { get; set; }

        public string SubscriptionName { get; set; }

        public string OfferId { get; set; }

        public string PlanId { get; set; }

        public int? Quantity { get; set; }
    }
}
