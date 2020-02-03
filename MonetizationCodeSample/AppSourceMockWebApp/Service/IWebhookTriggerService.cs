// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace AppSourceMockWebApp.Service
{
    using System.Threading.Tasks;
    using AppSourceMockWebApp.Models;

    public interface IWebhookTriggerService
    {
        Task NotifyAsync(OperationUpdate notification);
    }
}
