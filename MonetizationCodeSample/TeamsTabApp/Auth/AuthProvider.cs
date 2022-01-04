// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace TeamsTabApp.Auth
{
    using System;
    using System.Threading.Tasks;

    using Microsoft.Identity.Client;
    using Microsoft.Extensions.Options;
    using Microsoft.Graph;

    public interface IAuthProvider
    {
        Task<string> GetUserAccessTokenAsync(string jwtBearerToken);
    }

    public class AuthProvider : IAuthProvider
    {
        private readonly IConfidentialClientApplication _oboApp;
        private readonly string[] _scopes;

        public AuthProvider(IOptions<AzureAdOptions> azureAdOption)
        {
            var option = azureAdOption.Value;
            // More info about MSAL Client Applications: https://github.com/AzureAD/microsoft-authentication-library-for-dotnet/wiki/Client-Applications
            _oboApp = ConfidentialClientApplicationBuilder.Create(option.ClientId)
                    .WithClientSecret(option.ClientSecret)
                    .WithAuthority(AzureCloudInstance.AzurePublic, AadAuthorityAudience.AzureAdAndPersonalMicrosoftAccount)
                    .Build();

            _scopes = option.SaaSScopes.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
        }

        public async Task<string> GetUserAccessTokenAsync(string jwtBearerToken)
        {
            var account = await _oboApp.AcquireTokenOnBehalfOf(_scopes, new UserAssertion(jwtBearerToken)).ExecuteAsync();
            if (account == null) throw new ServiceException(new Error
            {
                Code = "TokenNotFound",
                Message = "Unable to retrieve the access token from assertion"
            });

            return account.AccessToken;
        }
    }
}
