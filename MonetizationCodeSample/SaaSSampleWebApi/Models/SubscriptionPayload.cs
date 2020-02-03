// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace SaaSSampleWebApi.Models
{
    using Newtonsoft.Json;

    public class SubscriptionPayload
    {
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string PlanId { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public int? Quantity { get; set; }
    }
}
