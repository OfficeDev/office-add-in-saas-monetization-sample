// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace OutlookAddInWeb.Services
{
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using OutlookAddInWeb.Models;

    public static class Bootstrapper
    {
        public static void AddApiService(this IServiceCollection services, IConfiguration configuration)
        {
            services.Configure<AzureAdOptions>(configuration.GetSection("AzureAd"));
        }
    }
}
