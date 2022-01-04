// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace SaaSSampleWebApp.Services
{
    using System;
    using System.Linq;
    using System.Threading.Tasks;

    using Microsoft.Extensions.Options;
    using Microsoft.Graph;
    using Microsoft.Identity.Web;
    using SaaSSampleWebApp.Extensions;
    

    public class MSGraphHelper : IMSGraphHelper
    {
        private readonly AzureAdOptions _azureAdOptions;
        private readonly ITokenAcquisition _tokenAcquisition;

        public MSGraphHelper(IOptions<AzureAdOptions> azureAdOptions, ITokenAcquisition tokenAcquisition)
        {
            _azureAdOptions = azureAdOptions.Value;
            _tokenAcquisition = tokenAcquisition;
        }

        public async Task<bool> IsTenantAdminAsync()
        {
            var graphClient = GetGraphServiceClient();

            var pagedItems = await graphClient.Me.MemberOf.Request().GetAsync();

            foreach (var directObject in pagedItems.CurrentPage)
            {
                if (directObject is DirectoryRole
                    && (directObject as DirectoryRole).RoleTemplateId  == "62e90394-69f5-4237-9190-012177145e10")
                    return true;
            }

            return false;
        }

        public async Task<Organization> GetOrganizationAsync()
        {
            var graphClient = GetGraphServiceClient();

            var result = await graphClient.Organization.Request().GetAsync();

            return result.CurrentPage.FirstOrDefault();
        }

        public async Task<User> GetUserAsync(string identity)
        {
            var graphClient = GetGraphServiceClient();

            var reuslt = await graphClient.Users.Request().Filter($"userPrincipalName eq '{identity}' and userType eq 'Member'").GetAsync();

            return reuslt.FirstOrDefault();
        }

        public Task<User> GetUserAsync(Guid objectId)
        {
            var graphClient = GetGraphServiceClient();

            return graphClient.Users[objectId.ToString()].Request().GetAsync();
        }

        private GraphServiceClient GetGraphServiceClient()
        {
            return GraphServiceClientFactory.GetAuthenticatedGraphClient(async () =>
            {
                string result = await _tokenAcquisition.GetAccessTokenOnBehalfOfUserAsync(_azureAdOptions.GraphScopes.Split(' '));
                return result;
            }, _azureAdOptions.GraphApiUrl);
        }
    }

    public interface IMSGraphHelper
    {
        Task<bool> IsTenantAdminAsync();
        Task<Organization> GetOrganizationAsync();
        Task<User> GetUserAsync(string identity);
        Task<User> GetUserAsync(Guid objectId);
    }
}
