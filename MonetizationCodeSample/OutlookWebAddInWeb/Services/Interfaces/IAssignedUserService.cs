// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace OutlookWebAddInWeb.Services
{
    using System.Threading.Tasks;
    using OutlookWebAddInWeb.Models;

    public interface IAssignedUserService
    {
        Task<Activation> Activate(string url, string accesstoken);
    }
}
