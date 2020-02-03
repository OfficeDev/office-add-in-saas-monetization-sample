// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace SaaSSampleWebApi.Models
{
    using System.Text.Json.Serialization;

    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum LicenceType
    {
        SeatBased,
        SiteBased
    }
}
