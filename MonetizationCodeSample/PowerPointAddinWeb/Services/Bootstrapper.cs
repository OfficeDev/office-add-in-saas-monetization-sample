// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace PowerPointAddInWeb.Services
{
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using PowerPointAddInWeb.Models;

    public static class Bootstrapper
    {
        public static void AddApiService(this IServiceCollection services, IConfiguration configuration)
        {
            services.Configure<AzureAdOptions>(configuration.GetSection("AzureAd"));
        }
    }
}
