// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace ExcelAddInWeb.Services
{
    using System.Net.Http;
    using System.Net.Http.Headers;
    using System.Threading.Tasks;

    using Newtonsoft.Json;
    using ExcelAddInWeb.Models;

    public class AssignedUserService: IAssignedUserService
    {
        public async Task<Activation> Activate(string url, string accesstoken) {
            
            Activation activation = null;

            using (var client = new HttpClient())
            {
                // Create and send the HTTP Request
                using (var request = new HttpRequestMessage(HttpMethod.Post, url))
                {
                    request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accesstoken);

                    using (HttpResponseMessage response = await client.SendAsync(request))
                    {
                        if (response.IsSuccessStatusCode)
                        {
                            HttpContent content = response.Content;
                            string responseContent = await content.ReadAsStringAsync();

                            activation = JsonConvert.DeserializeObject<Activation>(responseContent);
                        }
                    }
                }
            }
            return activation;
        }
    }
}
