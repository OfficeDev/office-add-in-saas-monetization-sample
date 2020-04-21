// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace WordAddInWeb.Services
{
    using System.Threading.Tasks;
    using WordAddInWeb.Models;

    public interface IAssignedUserService
    {
        Task<Activation> Activate(string url, string accesstoken);
    }
}
