// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace SaaSSampleWebApp.Models
{
    using System;

    public class LicenseManager
    {
        public Guid UserId { get; set; }
        public bool IsAdmin { get; set; }
    }
}
