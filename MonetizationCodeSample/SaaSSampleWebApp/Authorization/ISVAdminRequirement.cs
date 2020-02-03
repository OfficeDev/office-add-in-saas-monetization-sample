// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace SaaSSampleWebApp.Authorization
{
    using Microsoft.AspNetCore.Authorization;

    public class ISVAdminRequirement : IAuthorizationRequirement
    {
        public string ISVAdminName { get; }

        public ISVAdminRequirement(string iSVAdminName)
        {
            ISVAdminName = iSVAdminName;
        }
    }
}
