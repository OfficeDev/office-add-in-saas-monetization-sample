// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace OutlookAddInWeb.Services
{
    using System.Threading.Tasks;
    using OutlookAddInWeb.Models;

    public interface IAssignedUserService
    {
        Task<Activation> Activate(string url, string accesstoken);
    }
}
