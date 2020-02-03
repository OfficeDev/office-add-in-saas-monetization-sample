// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace SaaSSampleWebApp.Services
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Net;
    using System.Net.Http;
    using System.Net.Http.Headers;
    using System.Threading.Tasks;

    using Microsoft.Extensions.Configuration;
    using Microsoft.Identity.Web;
    using Newtonsoft.Json;
    using SaaSSampleWebApp.Models;

    public class SubscriptionService : ISubscriptionService
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IConfiguration _configuration;
        private readonly ITokenAcquisition _tokenAcquisition;
        
        public SubscriptionService(
            IHttpClientFactory httpClientFactory,
            IConfiguration configuration,
            ITokenAcquisition tokenAcquisition
            )
        {
            _httpClientFactory = httpClientFactory;
            _configuration = configuration;
            _tokenAcquisition = tokenAcquisition;
        }

        [AuthorizeForScopes(ScopeKeySection = "SaaSWebApi:Scopes")]
        public async Task<Subscription> GetSubscriptionAsync(Guid tenantId)
        {
            var token = _tokenAcquisition.GetAccessTokenOnBehalfOfUserAsync(_configuration["SaaSWebApi:Scopes"].Split(' '));

            using var requestMessage = new HttpRequestMessage(HttpMethod.Get,
                $"{_configuration["SaaSWebApi:BaseAddress"]}/api/Subscriptions");

            requestMessage.Headers.Authorization =new AuthenticationHeaderValue("Bearer", await token);

            var httpClient = _httpClientFactory.CreateClient();
            var response = await httpClient.SendAsync(requestMessage);
            var json = await response.Content.ReadAsStringAsync();

            var subscription = JsonConvert.DeserializeObject<List<Subscription>>(json)
                .Where(subscription => subscription.TenantId == tenantId).FirstOrDefault();

            return subscription;
        }

        [AuthorizeForScopes(ScopeKeySection = "SaaSWebApi:Scopes")]
        public async Task<LicenseManager> CheckLicenseManagerAsync(Guid subscriptionId, Guid userId)
        {
            var token = _tokenAcquisition.GetAccessTokenOnBehalfOfUserAsync(_configuration["SaaSWebApi:Scopes"].Split(' '));

            using var requestMessage = new HttpRequestMessage(HttpMethod.Get,
                $"{_configuration["SaaSWebApi:BaseAddress"]}/api/Subscriptions/{subscriptionId}/licenseManagers");

            // the token is not required for Mock APIs 
            requestMessage.Headers.Authorization = new AuthenticationHeaderValue("Bearer", await token);

            var httpClient = _httpClientFactory.CreateClient();
            var response = await httpClient.SendAsync(requestMessage);

            if (response.StatusCode == HttpStatusCode.OK)
            {
                var result = await response.Content.ReadAsStringAsync();
                var licenseManager = JsonConvert.DeserializeObject<LicenseManager>(result);
                return licenseManager;
            }

            return null; 
        }
    }
}
