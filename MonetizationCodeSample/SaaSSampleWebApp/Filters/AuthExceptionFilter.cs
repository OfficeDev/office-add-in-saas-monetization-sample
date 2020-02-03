// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace SaaSSampleWebApp.Filters
{
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Authentication;
    using Microsoft.AspNetCore.Mvc.Filters;

    public class AuthExceptionFilter : IAsyncExceptionFilter
    {
        public async Task OnExceptionAsync(ExceptionContext context)
        {
            if (context.Exception is Microsoft.Identity.Client.MsalUiRequiredException ||
                (context.Exception.InnerException != null
                && context.Exception.InnerException is Microsoft.Identity.Client.MsalUiRequiredException))
            {
                context.ExceptionHandled = true;
                var request = context.HttpContext.Request;
                var appBaseUrl = $"{request.Scheme}://{request.Host}{request.PathBase}";
                var azureLogInUrl = $"{appBaseUrl}/AzureAD/Account/SignIn";
                await request.HttpContext.SignOutAsync();
                context.HttpContext.Response.Redirect(azureLogInUrl);
            }
        }
    }
}
