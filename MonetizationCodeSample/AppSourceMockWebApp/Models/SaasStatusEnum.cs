// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace AppSourceMockWebApp.Models
{
    using System.Text.Json.Serialization;

    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum SaasStatusEnum
    {
        NotStarted,
        InProgress,
        Succeeded,
        Failed,
        Conflict
    }
}
