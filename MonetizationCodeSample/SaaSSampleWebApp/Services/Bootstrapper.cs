// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace SaaSSampleWebApp.Services
{
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using SaaSSampleWebApp.Extensions;

    public static class Bootstrapper
    {
        public static void AddApiService(this IServiceCollection services, IConfiguration configuration)
        {
            services.Configure<AzureAdOptions>(configuration.GetSection("AzureAd"));

            services.Configure<Iconfiguration>(configuration.GetSection("SaaSWebApi"));

            services.AddTransient<IMSGraphHelper, MSGraphHelper>();
        }
    }
}
