// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace AppSourceMockWebApp.Models
{
    using System.Text.Json.Serialization;

    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum SaaSActionEnum
    {
        Unsubscribe,
        ChangePlan,
        ChangeQuantity,
        Suspend,
        Reinstate
    }
}
