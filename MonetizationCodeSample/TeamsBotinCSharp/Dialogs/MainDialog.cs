// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

namespace TeamsBotinCSharp
{
    using System.Net.Http;
    using System.Net.Http.Headers;
    using System.Threading;
    using System.Threading.Tasks;

    using Microsoft.Bot.Builder;
    using Microsoft.Bot.Builder.Dialogs;
    using Microsoft.Bot.Schema;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.Logging;
    using Newtonsoft.Json;
    using SaaSOfferMockData;
    using TeamsBotinCSharp.Models;

    public class MainDialog : LogoutDialog
    {
        protected readonly ILogger Logger;
        protected readonly IConfiguration Configuration;

        public MainDialog(IConfiguration configuration, ILogger<MainDialog> logger)
            : base(nameof(MainDialog), configuration["ConnectionName"])
        {
            Logger = logger;
            Configuration = configuration;

            AddDialog(new OAuthPrompt(
                nameof(OAuthPrompt),
                new OAuthPromptSettings
                {
                    ConnectionName = ConnectionName,
                    Text = "Please Sign In",
                    Title = "Sign In",
                    Timeout = 300000, // User has 5 minutes to login (1000 * 60 * 5)
                }));

            AddDialog(new ConfirmPrompt(nameof(ConfirmPrompt)));

            AddDialog(new WaterfallDialog(nameof(WaterfallDialog), new WaterfallStep[]
            {
                PromptStepAsync,
                LoginStepAsync
            }));

            // The initial child Dialog to run.
            InitialDialogId = nameof(WaterfallDialog);
        }

        private async Task<DialogTurnResult> PromptStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            return await stepContext.BeginDialogAsync(nameof(OAuthPrompt), null, cancellationToken);
        }

        private async Task<DialogTurnResult> LoginStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            // Get the token from the previous step. Note that we could also have gotten the
            // token directly from the prompt itself. There is an example of this in the next method.
            var tokenResponse = (TokenResponse)stepContext.Result;
            if (tokenResponse?.Token != null)
            {
                // Validate user license
                var activation = await Activate($"{(Configuration["SaaSAPI"])}/{Offers.ContosoAppOffer.OfferID}", tokenResponse.Token);

                await stepContext.Context.SendActivityAsync(activation.Status == ActivationStatus.Failure ? "You don't have a paid license " : "You do have a paid license ");

                await stepContext.Context.SendActivityAsync($"DEBUG: You have {(activation.AvailableLicenseQuantity.HasValue ? activation.AvailableLicenseQuantity.ToString() : "0")} licenses available in your tenant");

                await stepContext.Context.SendActivityAsync($"DEBUG: { activation.Reason }");

                await stepContext.Context.SendActivityAsync($"DEBUG: Overrun is {(activation.AllowOverAssignment == false ? "disabled" : "enabled")}");

                await stepContext.Context.SendActivityAsync($"DEBUG: Auto-license-assignment is {(activation.FirstComeFirstServedAssignment == true ? "enabled" : "disabled")}");

                return await stepContext.EndDialogAsync(cancellationToken: cancellationToken);
            }

            await stepContext.Context.SendActivityAsync(MessageFactory.Text("Login was not successful please try again."), cancellationToken);
            return await stepContext.EndDialogAsync(cancellationToken: cancellationToken);
        }

        public async Task<Activation> Activate(string url, string accesstoken)
        {
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
