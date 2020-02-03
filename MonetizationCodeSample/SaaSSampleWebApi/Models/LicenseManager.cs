// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace SaaSSampleWebApi.Models
{
    using System;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;

    public class LicenseManager
    {
        [Key]
        public Guid Id { get; set; }

        public Guid SubscriptionId { get; set; }

        [ForeignKey("SubscriptionId")]
        public Subscription Subscription { get; set; }

        public Guid UserId { get; set; }

        public string UserEmail { get; set; }

        public bool IsAdmin { get; set; }
    }
}
