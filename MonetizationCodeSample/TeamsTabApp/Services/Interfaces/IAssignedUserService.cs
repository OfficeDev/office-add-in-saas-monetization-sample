// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace TeamsTabApp.Services
{
    using System.Threading.Tasks;
    using TeamsTabApp.Models;

    public interface IAssignedUserService
    {
        Task<Activation> Activate(string url, string accesstoken);
    }
}

