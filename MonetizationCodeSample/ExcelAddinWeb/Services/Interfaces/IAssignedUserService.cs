// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace ExcelAddInWeb.Services
{
    using System.Threading.Tasks;
    using ExcelAddInWeb.Models;

    public interface IAssignedUserService
    {
        Task<Activation> Activate(string url, string accesstoken);
    }
}
