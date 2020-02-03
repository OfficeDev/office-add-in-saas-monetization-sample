// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace SaaSSampleWebApi.Models
{
    using System;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;

    using Newtonsoft.Json;

    public class Subscription
    {
        [Key]
        public Guid Id { get; set; }

        public string SubscriptionName { get; set; }

        public Guid TenantId { get; set; }

        public string TenantName { get; set; }

        [NotMapped]
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public LicenceType LicenceType
        {
            get
            {
                return PurchaseSeatsCount.HasValue ? LicenceType.SeatBased : LicenceType.SiteBased;
            }
        }

        public string OfferId { get; set; }

        public string PlanId { get; set; }

        [JsonIgnore]
        public Guid PurchaserId { get; set; }

        [JsonIgnore]
        public string Purchaser { get; set; }

        public int? PurchaseSeatsCount { get; set; }

        public bool AllowOverAssignment { get; set; } = false;

        public bool FirstComeFirstServedAssignment { get; set; } = false;

        public string Location { get; set; }
    }
}
