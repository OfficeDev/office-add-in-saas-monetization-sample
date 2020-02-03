// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace SaaSSampleWebApp.Authorization
{
    using System.Threading.Tasks;

    using Microsoft.AspNetCore.Authorization;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Identity.Web;

    /// <summary>
    /// check if the current user is ISV admin
    /// </summary>
    public class ISVAdminHandler : AuthorizationHandler<ISVAdminRequirement>
    {
        private readonly IConfiguration _configuration;

        public ISVAdminHandler(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        protected override Task HandleRequirementAsync(
           AuthorizationHandlerContext context,
           ISVAdminRequirement requirement)
        {
            if (context.User == null) return Task.CompletedTask;

            if (context.User.GetTenantId()
                == _configuration["AzureAd:DirectoryId"])
            {
                context.Succeed(requirement);
            }

            return Task.CompletedTask;
        }
    }
}
