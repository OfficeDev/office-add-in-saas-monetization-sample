// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace SaaSSampleWebApi.Models
{
    using System;

    public class WebhookPayload
    {
        public Guid Id { get; set; }

        public Guid ActivityId { get; set; }

        public Guid SubscriptionId { get; set; }

        public string PublisherId { get; set; }

        public string OfferId { get; set; }

        public string PlanId { get; set; }

        public int? Quantity { get; set; }

        public DateTime TimeStamp { get; set; }

        public string Action { get; set; }

        public string Status { get; set; }
    }
}
