// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace TeamsTabApp.Auth
{
    using System.Threading.Tasks;

    using Microsoft.AspNetCore.Http;
    using Microsoft.Extensions.Caching.Memory;
    using Microsoft.Extensions.Primitives;

    public interface ITokenService
    {
        Task<string> GetAccessToken();

        Task<string> GetAccessToken(string tuid);
    }

    public class TokenService : ITokenService
    {
        private readonly IAuthProvider _authProvider;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IMemoryCache _memoryCache;

        public TokenService(IAuthProvider authProvider, IHttpContextAccessor httpContextAccessor, IMemoryCache memoryCache)
        {
            _authProvider = authProvider;
            _httpContextAccessor = httpContextAccessor;
            _memoryCache = memoryCache;
        }

        private string GetAssertion()
        {
            var httpContext = _httpContextAccessor.HttpContext;
            httpContext.Request.Headers.TryGetValue("Authorization", out StringValues returnValue);
            return returnValue;
        }

        public Task<string> GetAccessToken()
        {
            var assertion = GetAssertion();

            return GetAccessToken(assertion);
        }

        public Task<string> GetAccessToken(string tuid)
        {
            return _memoryCache.GetOrCreateAsync(tuid, (o) => _authProvider.GetUserAccessTokenAsync(tuid));
        }
    }
}
