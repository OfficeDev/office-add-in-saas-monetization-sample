// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace AppSourceMockWebApp.Models
{
    using System;
    using Newtonsoft.Json;

    public class ResolvedSubscription
    {
        [JsonProperty(PropertyName = "id")]
        public Guid SubscriptionId { get; set; }

        public string SubscriptionName { get; set; }

        public string OfferId { get; set; }

        public string PlanId { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public int? Quantity { get; set; }

        [JsonIgnore]
        public Guid TenantId { get; set; }
    }
}
