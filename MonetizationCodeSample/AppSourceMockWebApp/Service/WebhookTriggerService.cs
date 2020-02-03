// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace AppSourceMockWebApp.Service
{
    using System.Net.Http;
    using System.Text;
    using System.Threading.Tasks;

    using AppSourceMockWebApp.Models;
    using Microsoft.Extensions.Configuration;
    using Newtonsoft.Json;

    // this service sends notifaction to webhook api configured in SaaS Offer configurations.
    public class WebhookTriggerService : IWebhookTriggerService
    {
        private readonly IConfiguration _configuration;
        private readonly IHttpClientFactory _httpClientFactory;

        public WebhookTriggerService(IConfiguration configuration, IHttpClientFactory httpClientFactory)
        {
            _configuration = configuration;
            _httpClientFactory = httpClientFactory;
        }

        public async Task NotifyAsync(OperationUpdate notification)
        {
            using var requestMessage = new HttpRequestMessage(
                HttpMethod.Post,
                _configuration["SaaSOffer:Connectionwebhook"]);

            var json = JsonConvert.SerializeObject(notification);
            var httpClient = _httpClientFactory.CreateClient();
            requestMessage.Content = new StringContent(json, Encoding.Default, "application/json");

            using var response = await httpClient.SendAsync(requestMessage).ConfigureAwait(false);
            response.EnsureSuccessStatusCode();
        }
    }
}
