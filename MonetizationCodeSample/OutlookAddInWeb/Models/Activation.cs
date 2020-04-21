// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace OutlookAddInWeb.Models
{
    using System.Text.Json.Serialization;

    public class Activation
    {
        public ActivationStatus Status { get; set; }

        public string Reason { get; set; }

        public bool AllowOverAssignment { get; set; }

        public bool FirstComeFirstServedAssignment { get; set; }

        public int? AvailableLicenseQuantity { get; set; }

        public int? LicenseAmount { get; set; }
    }

    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum ActivationStatus
    {
        Success,
        Failure
    }
}
