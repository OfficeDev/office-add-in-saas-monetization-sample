// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace ExcelAddInWeb.Services
{
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using ExcelAddInWeb.Models;

    public static class Bootstrapper
    {
        public static void AddApiService(this IServiceCollection services, IConfiguration configuration)
        {
            services.Configure<AzureAdOptions>(configuration.GetSection("AzureAd"));
        }
    }
}
