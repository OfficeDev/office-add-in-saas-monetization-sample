// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.
using System.Threading.Tasks;
using ProviderHostedAddInWeb.Models;

namespace ProviderHostedAddInWeb.Services
{
    public interface IAssignedUserService
    {
        Task<Activation> Activate(string url, string accesstoken);
    }
}
