// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace SaaSSampleWebApi.Models
{
    public class LicenseStatus
    {
        public ActivationStatusEnum Status { get; set; }

        public string Reason { get; set; }

        public bool AllowOverAssignment { get; set; }

        public bool FirstComeFirstServedAssignment { get; set; }

        public int? AvailableLicenseQuantity { get; set; }

        public int? LicenseAmount { get; set; }
    } 
}
