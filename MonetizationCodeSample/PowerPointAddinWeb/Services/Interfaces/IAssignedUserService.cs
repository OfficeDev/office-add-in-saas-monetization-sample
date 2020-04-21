// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace PowerPointAddInWeb.Services
{
    using System.Threading.Tasks;
    using PowerPointAddInWeb.Models;

    public interface IAssignedUserService
    {
        Task<Activation> Activate(string url, string accesstoken);
    }
}
