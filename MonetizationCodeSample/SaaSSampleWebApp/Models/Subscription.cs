// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace SaaSSampleWebApp.Models
{
    using System;

    public class Subscription
    {
        public Guid Id { get; set; }

        public Guid TenantId { get; set; }

        public string LicenceType { get; set; }
    }
}
