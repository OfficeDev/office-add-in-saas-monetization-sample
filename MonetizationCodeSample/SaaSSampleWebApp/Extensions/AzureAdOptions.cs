// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace SaaSSampleWebApp.Extensions
{
    public class AzureAdOptions
    {
        public string ClientId { get; set; }

        public string ClientSecret { get; set; }

        public string Instance { get; set; }

        public string Domain { get; set; }

        public string TenantId { get; set; }

        public string CallbackPath { get; set; }

        public string BaseUrl { get; set; }

        public string Scopes { get; set; }

        public string GraphApiUrl { get; set; }

        public string GraphScopes { get; set; }

        public string DirectoryId { get; set; }
    }
}
