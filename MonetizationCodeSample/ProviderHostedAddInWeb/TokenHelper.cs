using Microsoft.SharePoint.Client;
using Microsoft.SharePoint.Client.EventReceivers;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Globalization;
using System.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.IO;
using System.Linq;
using System.Net;
using System.Security.Claims;
using System.Security.Cryptography.X509Certificates;
using System.Security.Principal;
using System.ServiceModel;
using System.Text;
using System.Web;
using System.Web.Configuration;
using System.Web.Script.Serialization;
using SigningCredentials = Microsoft.IdentityModel.Tokens.SigningCredentials;
using SymmetricSecurityKey = Microsoft.IdentityModel.Tokens.SymmetricSecurityKey;
using TokenValidationParameters = Microsoft.IdentityModel.Tokens.TokenValidationParameters;
using X509SigningCredentials = Microsoft.IdentityModel.Tokens.X509SigningCredentials;

namespace ProviderHostedAddInWeb
{
    public static class TokenHelper
    {
        #region public fields

        /// <summary>
        /// SharePoint principal.
        /// </summary>
        public const string SharePointPrincipal = "00000003-0000-0ff1-ce00-000000000000";

        /// <summary>
        /// Lifetime of HighTrust access token, 12 hours.
        /// </summary>
        public static readonly TimeSpan HighTrustAccessTokenLifetime = TimeSpan.FromHours(12.0);

        #endregion public fields

        #region public methods

        /// <summary>
        /// Retrieves the context token string from the specified request by looking for well-known parameter names in the
        /// POSTed form parameters and the querystring. Returns null if no context token is found.
        /// </summary>
        /// <param name="request">HttpRequest in which to look for a context token</param>
        /// <returns>The context token string</returns>
        public static string GetContextTokenFromRequest(HttpRequest request)
        {
            return GetContextTokenFromRequest(new HttpRequestWrapper(request));
        }

        /// <summary>
        /// Retrieves the context token string from the specified request by looking for well-known parameter names in the
        /// POSTed form parameters and the querystring. Returns null if no context token is found.
        /// </summary>
        /// <param name="request">HttpRequest in which to look for a context token</param>
        /// <returns>The context token string</returns>
        public static string GetContextTokenFromRequest(HttpRequestBase request)
        {
            string[] paramNames = { "AppContext", "AppContextToken", "AccessToken", "SPAppToken" };
            foreach (string paramName in paramNames)
            {
                if (!string.IsNullOrEmpty(request.Form[paramName]))
                {
                    return request.Form[paramName];
                }
                if (!string.IsNullOrEmpty(request.QueryString[paramName]))
                {
                    return request.QueryString[paramName];
                }
            }
            return null;
        }

        /// <summary>
        /// Validate that a specified context token string is intended for this application based on the parameters
        /// specified in web.config. Parameters used from web.config used for validation include ClientId,
        /// HostedAppHostNameOverride, HostedAppHostName, ClientSecret, and Realm (if it is specified). If HostedAppHostNameOverride is present,
        /// it will be used for validation. Otherwise, if the <paramref name="appHostName"/> is not
        /// null, it is used for validation instead of the web.config's HostedAppHostName. If the token is invalid, an
        /// exception is thrown. If the token is valid, TokenHelper's static STS metadata url is updated based on the token contents
        /// and a JwtSecurityToken based on the context token is returned.
        /// </summary>
        /// <param name="contextTokenString">The context token to validate</param>
        /// <param name="appHostName">The URL authority, consisting of  Domain Name System (DNS) host name or IP address and the port number, to use for token audience validation.
        /// If null, HostedAppHostName web.config setting is used instead. HostedAppHostNameOverride web.config setting, if present, will be used
        /// for validation instead of <paramref name="appHostName"/> .</param>
        /// <returns>A JwtSecurityToken based on the context token.</returns>
        public static SharePointContextToken ReadAndValidateContextToken(string contextTokenString, string appHostName = null)
        {
            List<SymmetricSecurityKey> securityKeys = new List<SymmetricSecurityKey>
            {
                new SymmetricSecurityKey(Convert.FromBase64String(ClientSecret))
            };

            if (!string.IsNullOrEmpty(SecondaryClientSecret))
            {
                securityKeys.Add(new SymmetricSecurityKey(Convert.FromBase64String(SecondaryClientSecret)));
            }

            JwtSecurityTokenHandler tokenHandler = CreateJwtSecurityTokenHandler();
            TokenValidationParameters parameters = new TokenValidationParameters
            {
                ValidateIssuer = false,
                ValidateAudience = false, // validated below
                IssuerSigningKeys = securityKeys // validate the signature
            };

            tokenHandler.ValidateToken(contextTokenString, parameters, out Microsoft.IdentityModel.Tokens.SecurityToken securityToken);
            SharePointContextToken token = SharePointContextToken.Create(securityToken as JwtSecurityToken);

            string stsAuthority = (new Uri(token.SecurityTokenServiceUri)).Authority;
            int firstDot = stsAuthority.IndexOf('.');

            GlobalEndPointPrefix = stsAuthority.Substring(0, firstDot);
            AcsHostUrl = stsAuthority.Substring(firstDot + 1);

            string[] acceptableAudiences;
            if (!String.IsNullOrEmpty(HostedAppHostNameOverride))
            {
                acceptableAudiences = HostedAppHostNameOverride.Split(';');
            }
            else if (appHostName == null)
            {
                acceptableAudiences = new[] { HostedAppHostName };
            }
            else
            {
                acceptableAudiences = new[] { appHostName };
            }

            bool validationSuccessful = false;
            string realm = Realm ?? token.Realm;
            foreach (var audience in acceptableAudiences)
            {
                string principal = GetFormattedPrincipal(ClientId, audience, realm);
                if (token.Audiences.First<string>(item => StringComparer.OrdinalIgnoreCase.Equals(item, principal)) != null)
                {
                    validationSuccessful = true;
                    break;
                }
            }

            if (!validationSuccessful)
            {
                throw new AudienceUriValidationFailedException(
                    String.Format(CultureInfo.CurrentCulture,
                    "\"{0}\" is not the intended audience \"{1}\"", String.Join(";", acceptableAudiences),
                    String.Join(";", token.Audiences.ToArray<string>())));
            }

            return token;
        }

        /// <summary>
        /// Retrieves an access token from ACS to call the source of the specified context token at the specified
        /// targetHost. The targetHost must be registered for the principal that sent the context token.
        /// </summary>
        /// <param name="contextToken">Context token issued by the intended access token audience</param>
        /// <param name="targetHost">Url authority of the target principal</param>
        /// <returns>An access token with an audience matching the context token's source</returns>
        public static OAuthTokenResponse GetAccessToken(SharePointContextToken contextToken, string targetHost)
        {
            string targetPrincipalName = contextToken.TargetPrincipalName;

            // Extract the refreshToken from the context token
            string refreshToken = contextToken.RefreshToken;

            if (String.IsNullOrEmpty(refreshToken))
            {
                return null;
            }

            string targetRealm = Realm ?? contextToken.Realm;

            return GetAccessToken(refreshToken,
                                  targetPrincipalName,
                                  targetHost,
                                  targetRealm);
        }

        /// <summary>
        /// Uses the specified authorization code to retrieve an access token from ACS to call the specified principal
        /// at the specified targetHost. The targetHost must be registered for target principal.  If specified realm is
        /// null, the "Realm" setting in web.config will be used instead.
        /// </summary>
        /// <param name="authorizationCode">Authorization code to exchange for access token</param>
        /// <param name="targetPrincipalName">Name of the target principal to retrieve an access token for</param>
        /// <param name="targetHost">Url authority of the target principal</param>
        /// <param name="targetRealm">Realm to use for the access token's nameid and audience</param>
        /// <param name="redirectUri">Redirect URI registered for this add-in</param>
        /// <returns>An access token with an audience of the target principal</returns>
        public static OAuthTokenResponse GetAccessToken(
            string authorizationCode,
            string targetPrincipalName,
            string targetHost,
            string targetRealm,
            Uri redirectUri)
        {
            if (targetRealm == null)
            {
                targetRealm = Realm;
            }

            string resource = GetFormattedPrincipal(targetPrincipalName, targetHost, targetRealm);
            string clientId = GetFormattedPrincipal(ClientId, null, targetRealm);
            string acsUri = AcsMetadataParser.GetStsUrl(targetRealm);
            OAuthTokenResponse oauthResponse = null;

            try
            {
                oauthResponse = OAuthClient.GetAccessTokenWithAuthorizationCode(acsUri, clientId, ClientSecret,
                    authorizationCode, redirectUri.AbsoluteUri, resource);
            }
            catch (WebException wex)
            {
                if (!string.IsNullOrEmpty(SecondaryClientSecret))
                {
                    oauthResponse = OAuthClient.GetAccessTokenWithAuthorizationCode(acsUri, clientId, SecondaryClientSecret,
                        authorizationCode, redirectUri.AbsoluteUri, resource);
                }
                else
                {
                    using (StreamReader sr = new StreamReader(wex.Response.GetResponseStream()))
                    {
                        string responseText = sr.ReadToEnd();
                        throw new WebException(wex.Message + " - " + responseText, wex);
                    }
                }
            }

            return oauthResponse;
        }

        /// <summary>
        /// Uses the specified refresh token to retrieve an access token from ACS to call the specified principal
        /// at the specified targetHost. The targetHost must be registered for target principal.  If specified realm is
        /// null, the "Realm" setting in web.config will be used instead.
        /// </summary>
        /// <param name="refreshToken">Refresh token to exchange for access token</param>
        /// <param name="targetPrincipalName">Name of the target principal to retrieve an access token for</param>
        /// <param name="targetHost">Url authority of the target principal</param>
        /// <param name="targetRealm">Realm to use for the access token's nameid and audience</param>
        /// <returns>An access token with an audience of the target principal</returns>
        public static OAuthTokenResponse GetAccessToken(
            string refreshToken,
            string targetPrincipalName,
            string targetHost,
            string targetRealm)
        {
            if (targetRealm == null)
            {
                targetRealm = Realm;
            }

            string resource = GetFormattedPrincipal(targetPrincipalName, targetHost, targetRealm);
            string clientId = GetFormattedPrincipal(ClientId, null, targetRealm);
            string acsUri = AcsMetadataParser.GetStsUrl(targetRealm);
            OAuthTokenResponse oauthResponse = null;

            try
            {
                oauthResponse = OAuthClient.GetAccessTokenWithRefreshToken(acsUri, clientId, ClientSecret, refreshToken, resource);
            }
            catch (WebException wex)
            {
                if (!string.IsNullOrEmpty(SecondaryClientSecret))
                {
                    oauthResponse = OAuthClient.GetAccessTokenWithRefreshToken(acsUri, clientId, SecondaryClientSecret, refreshToken, resource);
                }
                else
                {
                    using (StreamReader sr = new StreamReader(wex.Response.GetResponseStream()))
                    {
                        string responseText = sr.ReadToEnd();
                        throw new WebException(wex.Message + " - " + responseText, wex);
                    }
                }
            }

            return oauthResponse;
        }

        /// <summary>
        /// Retrieves an app-only access token from ACS to call the specified principal
        /// at the specified targetHost. The targetHost must be registered for target principal.  If specified realm is
        /// null, the "Realm" setting in web.config will be used instead.
        /// </summary>
        /// <param name="targetPrincipalName">Name of the target principal to retrieve an access token for</param>
        /// <param name="targetHost">Url authority of the target principal</param>
        /// <param name="targetRealm">Realm to use for the access token's nameid and audience</param>
        /// <returns>An access token with an audience of the target principal</returns>
        public static OAuthTokenResponse GetAppOnlyAccessToken(
            string targetPrincipalName,
            string targetHost,
            string targetRealm)
        {

            if (targetRealm == null)
            {
                targetRealm = Realm;
            }

            string resource = GetFormattedPrincipal(targetPrincipalName, targetHost, targetRealm);
            string clientId = GetFormattedPrincipal(ClientId, HostedAppHostName, targetRealm);
            string acsUri = AcsMetadataParser.GetStsUrl(targetRealm);
            OAuthTokenResponse oauthResponse = null;

            try
            {
                oauthResponse = OAuthClient.GetAccessTokenWithClientCredentials(acsUri, clientId, ClientSecret, resource);
            }
            catch (WebException wex)
            {
                if (!string.IsNullOrEmpty(SecondaryClientSecret))
                {
                    oauthResponse = OAuthClient.GetAccessTokenWithClientCredentials(acsUri, clientId, SecondaryClientSecret, resource);
                }
                else
                {
                    using (StreamReader sr = new StreamReader(wex.Response.GetResponseStream()))
                    {
                        string responseText = sr.ReadToEnd();
                        throw new WebException(wex.Message + " - " + responseText, wex);
                    }
                }
            }

            return oauthResponse;
        }

        /// <summary>
        /// Creates a client context based on the properties of a remote event receiver
        /// </summary>
        /// <param name="properties">Properties of a remote event receiver</param>
        /// <returns>A ClientContext ready to call the web where the event originated</returns>
        public static ClientContext CreateRemoteEventReceiverClientContext(SPRemoteEventProperties properties)
        {
            Uri sharepointUrl;
            if (properties.ListEventProperties != null)
            {
                sharepointUrl = new Uri(properties.ListEventProperties.WebUrl);
            }
            else if (properties.ItemEventProperties != null)
            {
                sharepointUrl = new Uri(properties.ItemEventProperties.WebUrl);
            }
            else if (properties.WebEventProperties != null)
            {
                sharepointUrl = new Uri(properties.WebEventProperties.FullUrl);
            }
            else
            {
                return null;
            }

            if (IsHighTrustApp())
            {
                return GetS2SClientContextWithWindowsIdentity(sharepointUrl, null);
            }

            return CreateAcsClientContextForUrl(properties, sharepointUrl);
        }

        /// <summary>
        /// Creates a client context based on the properties of an add-in event
        /// </summary>
        /// <param name="properties">Properties of an add-in event</param>
        /// <param name="useAppWeb">True to target the app web, false to target the host web</param>
        /// <returns>A ClientContext ready to call the app web or the parent web</returns>
        public static ClientContext CreateAppEventClientContext(SPRemoteEventProperties properties, bool useAppWeb)
        {
            if (properties.AppEventProperties == null)
            {
                return null;
            }

            Uri sharepointUrl = useAppWeb ? properties.AppEventProperties.AppWebFullUrl : properties.AppEventProperties.HostWebFullUrl;
            if (IsHighTrustApp())
            {
                return GetS2SClientContextWithWindowsIdentity(sharepointUrl, null);
            }

            return CreateAcsClientContextForUrl(properties, sharepointUrl);
        }

        /// <summary>
        /// Retrieves an access token from ACS using the specified authorization code, and uses that access token to
        /// create a client context
        /// </summary>
        /// <param name="targetUrl">Url of the target SharePoint site</param>
        /// <param name="authorizationCode">Authorization code to use when retrieving the access token from ACS</param>
        /// <param name="redirectUri">Redirect URI registered for this add-in</param>
        /// <returns>A ClientContext ready to call targetUrl with a valid access token</returns>
        public static ClientContext GetClientContextWithAuthorizationCode(
            string targetUrl,
            string authorizationCode,
            Uri redirectUri)
        {
            return GetClientContextWithAuthorizationCode(targetUrl, SharePointPrincipal, authorizationCode, GetRealmFromTargetUrl(new Uri(targetUrl)), redirectUri);
        }

        /// <summary>
        /// Retrieves an access token from ACS using the specified authorization code, and uses that access token to
        /// create a client context
        /// </summary>
        /// <param name="targetUrl">Url of the target SharePoint site</param>
        /// <param name="targetPrincipalName">Name of the target SharePoint principal</param>
        /// <param name="authorizationCode">Authorization code to use when retrieving the access token from ACS</param>
        /// <param name="targetRealm">Realm to use for the access token's nameid and audience</param>
        /// <param name="redirectUri">Redirect URI registered for this add-in</param>
        /// <returns>A ClientContext ready to call targetUrl with a valid access token</returns>
        public static ClientContext GetClientContextWithAuthorizationCode(
            string targetUrl,
            string targetPrincipalName,
            string authorizationCode,
            string targetRealm,
            Uri redirectUri)
        {
            Uri targetUri = new Uri(targetUrl);

            string accessToken =
                GetAccessToken(authorizationCode, targetPrincipalName, targetUri.Authority, targetRealm, redirectUri).AccessToken;

            return GetClientContextWithAccessToken(targetUrl, accessToken);
        }

        /// <summary>
        /// Uses the specified access token to create a client context
        /// </summary>
        /// <param name="targetUrl">Url of the target SharePoint site</param>
        /// <param name="accessToken">Access token to be used when calling the specified targetUrl</param>
        /// <returns>A ClientContext ready to call targetUrl with the specified access token</returns>
        public static ClientContext GetClientContextWithAccessToken(string targetUrl, string accessToken)
        {
            ClientContext clientContext = new ClientContext(targetUrl);

            clientContext.AuthenticationMode = ClientAuthenticationMode.Anonymous;
            clientContext.FormDigestHandlingEnabled = false;
            clientContext.ExecutingWebRequest +=
                delegate (object oSender, WebRequestEventArgs webRequestEventArgs)
                {
                    webRequestEventArgs.WebRequestExecutor.RequestHeaders["Authorization"] =
                        "Bearer " + accessToken;
                };

            return clientContext;
        }

        /// <summary>
        /// Retrieves an access token from ACS using the specified context token, and uses that access token to create
        /// a client context
        /// </summary>
        /// <param name="targetUrl">Url of the target SharePoint site</param>
        /// <param name="contextTokenString">Context token received from the target SharePoint site</param>
        /// <param name="appHostUrl">Url authority of the hosted add-in.  If this is null, the value in the HostedAppHostName
        /// of web.config will be used instead</param>
        /// <returns>A ClientContext ready to call targetUrl with a valid access token</returns>
        public static ClientContext GetClientContextWithContextToken(
            string targetUrl,
            string contextTokenString,
            string appHostUrl)
        {
            SharePointContextToken contextToken = ReadAndValidateContextToken(contextTokenString, appHostUrl);

            Uri targetUri = new Uri(targetUrl);

            string accessToken = GetAccessToken(contextToken, targetUri.Authority).AccessToken;

            return GetClientContextWithAccessToken(targetUrl, accessToken);
        }

        /// <summary>
        /// Returns the SharePoint url to which the add-in should redirect the browser to request consent and get back
        /// an authorization code.
        /// </summary>
        /// <param name="contextUrl">Absolute Url of the SharePoint site</param>
        /// <param name="scope">Space-delimited permissions to request from the SharePoint site in "shorthand" format
        /// (e.g. "Web.Read Site.Write")</param>
        /// <returns>Url of the SharePoint site's OAuth authorization page</returns>
        public static string GetAuthorizationUrl(string contextUrl, string scope)
        {
            return string.Format(
                "{0}{1}?IsDlg=1&client_id={2}&scope={3}&response_type=code",
                EnsureTrailingSlash(contextUrl),
                AuthorizationPage,
                ClientId,
                scope);
        }

        /// <summary>
        /// Returns the SharePoint url to which the add-in should redirect the browser to request consent and get back
        /// an authorization code.
        /// </summary>
        /// <param name="contextUrl">Absolute Url of the SharePoint site</param>
        /// <param name="scope">Space-delimited permissions to request from the SharePoint site in "shorthand" format
        /// (e.g. "Web.Read Site.Write")</param>
        /// <param name="redirectUri">Uri to which SharePoint should redirect the browser to after consent is
        /// granted</param>
        /// <returns>Url of the SharePoint site's OAuth authorization page</returns>
        public static string GetAuthorizationUrl(string contextUrl, string scope, string redirectUri)
        {
            return string.Format(
                "{0}{1}?IsDlg=1&client_id={2}&scope={3}&response_type=code&redirect_uri={4}",
                EnsureTrailingSlash(contextUrl),
                AuthorizationPage,
                ClientId,
                scope,
                redirectUri);
        }

        /// <summary>
        /// Returns the SharePoint url to which the add-in should redirect the browser to request a new context token.
        /// </summary>
        /// <param name="contextUrl">Absolute Url of the SharePoint site</param>
        /// <param name="redirectUri">Uri to which SharePoint should redirect the browser to with a context token</param>
        /// <returns>Url of the SharePoint site's context token redirect page</returns>
        public static string GetAppContextTokenRequestUrl(string contextUrl, string redirectUri)
        {
            return string.Format(
                "{0}{1}?client_id={2}&redirect_uri={3}",
                EnsureTrailingSlash(contextUrl),
                RedirectPage,
                ClientId,
                redirectUri);
        }

        /// <summary>
        /// Retrieves an S2S access token signed by the application's private certificate on behalf of the specified
        /// WindowsIdentity and intended for the SharePoint at the targetApplicationUri. If no Realm is specified in
        /// web.config, an auth challenge will be issued to the targetApplicationUri to discover it.
        /// </summary>
        /// <param name="targetApplicationUri">Url of the target SharePoint site</param>
        /// <param name="identity">Windows identity of the user on whose behalf to create the access token</param>
        /// <returns>An access token with an audience of the target principal</returns>
        public static string GetS2SAccessTokenWithWindowsIdentity(
            Uri targetApplicationUri,
            WindowsIdentity identity)
        {
            string realm = string.IsNullOrEmpty(Realm) ? GetRealmFromTargetUrl(targetApplicationUri) : Realm;

            Claim[] claims = identity != null ? GetClaimsWithWindowsIdentity(identity) : null;

            return GetS2SAccessTokenWithClaims(targetApplicationUri.Authority, realm, claims);
        }

        /// <summary>
        /// Retrieves an S2S client context with an access token signed by the application's private certificate on
        /// behalf of the specified WindowsIdentity and intended for application at the targetApplicationUri using the
        /// targetRealm. If no Realm is specified in web.config, an auth challenge will be issued to the
        /// targetApplicationUri to discover it.
        /// </summary>
        /// <param name="targetApplicationUri">Url of the target SharePoint site</param>
        /// <param name="identity">Windows identity of the user on whose behalf to create the access token</param>
        /// <returns>A ClientContext using an access token with an audience of the target application</returns>
        public static ClientContext GetS2SClientContextWithWindowsIdentity(
            Uri targetApplicationUri,
            WindowsIdentity identity)
        {
            string realm = string.IsNullOrEmpty(Realm) ? GetRealmFromTargetUrl(targetApplicationUri) : Realm;

            Claim[] claims = identity != null ? GetClaimsWithWindowsIdentity(identity) : null;

            string accessToken = GetS2SAccessTokenWithClaims(targetApplicationUri.Authority, realm, claims);

            return GetClientContextWithAccessToken(targetApplicationUri.ToString(), accessToken);
        }

        /// <summary>
        /// Get authentication realm from SharePoint
        /// </summary>
        /// <param name="targetApplicationUri">Url of the target SharePoint site</param>
        /// <returns>String representation of the realm GUID</returns>
        public static string GetRealmFromTargetUrl(Uri targetApplicationUri)
        {
            WebRequest request = WebRequest.Create(targetApplicationUri + "/_vti_bin/client.svc");
            request.Headers.Add("Authorization: Bearer ");

            try
            {
                using (request.GetResponse())
                {
                }
            }
            catch (WebException e)
            {
                if (e.Response == null)
                {
                    return null;
                }

                string bearerResponseHeader = e.Response.Headers["WWW-Authenticate"];
                if (string.IsNullOrEmpty(bearerResponseHeader))
                {
                    return null;
                }

                const string bearer = "Bearer realm=\"";
                int bearerIndex = bearerResponseHeader.IndexOf(bearer, StringComparison.Ordinal);
                if (bearerIndex < 0)
                {
                    return null;
                }

                int realmIndex = bearerIndex + bearer.Length;

                if (bearerResponseHeader.Length >= realmIndex + 36)
                {
                    string targetRealm = bearerResponseHeader.Substring(realmIndex, 36);

                    Guid realmGuid;

                    if (Guid.TryParse(targetRealm, out realmGuid))
                    {
                        return targetRealm;
                    }
                }
            }
            return null;
        }

        /// <summary>
        /// Determines if this is a high trust add-in.
        /// </summary>
        /// <returns>True if this is a high trust add-in.</returns>
        public static bool IsHighTrustApp()
        {
            return SigningCredentials != null;
        }

        /// <summary>
        /// Ensures that the specified URL ends with '/' if it is not null or empty.
        /// </summary>
        /// <param name="url">The url.</param>
        /// <returns>The url ending with '/' if it is not null or empty.</returns>
        public static string EnsureTrailingSlash(string url)
        {
            if (!string.IsNullOrEmpty(url) && url[url.Length - 1] != '/')
            {
                return url + "/";
            }

            return url;
        }

        /// <summary>
        /// Returns the current Epoch time in seconds
        /// </summary>
        /// <returns>Epoch time in seconds</returns>
        public static long EpochTimeNow()
        {
            return (long)(DateTime.UtcNow - new DateTime(1970, 1, 1).ToUniversalTime()).TotalSeconds;
        }

        #endregion

        #region private fields

        //
        // Configuration Constants
        //

        private const string AuthorizationPage = "_layouts/15/OAuthAuthorize.aspx";
        private const string RedirectPage = "_layouts/15/AppRedirect.aspx";
        private const string AcsPrincipalName = "00000001-0000-0000-c000-000000000000";
        private const string AcsMetadataEndPointRelativeUrl = "metadata/json/1";
        private const string S2SProtocol = "OAuth2";
        private const string DelegationIssuance = "DelegationIssuance1.0";
        private const string NameIdentifierClaimType = "nameid";
        private const string TrustedForImpersonationClaimType = "trustedfordelegation";
        private const string ActorTokenClaimType = "actortoken";

        //
        // Environment Constants
        //

        private static string GlobalEndPointPrefix = "accounts";
        private static string AcsHostUrl = "accesscontrol.windows.net";

        //
        // Hosted add-in configuration
        //
        private static readonly string ClientId = string.IsNullOrEmpty(WebConfigurationManager.AppSettings.Get("ClientId")) ? WebConfigurationManager.AppSettings.Get("HostedAppName") : WebConfigurationManager.AppSettings.Get("ClientId");
        private static readonly string IssuerId = string.IsNullOrEmpty(WebConfigurationManager.AppSettings.Get("IssuerId")) ? ClientId : WebConfigurationManager.AppSettings.Get("IssuerId");
        private static readonly string HostedAppHostNameOverride = WebConfigurationManager.AppSettings.Get("HostedAppHostNameOverride");
        private static readonly string HostedAppHostName = WebConfigurationManager.AppSettings.Get("HostedAppHostName");
        private static readonly string ClientSecret = string.IsNullOrEmpty(WebConfigurationManager.AppSettings.Get("ClientSecret")) ? WebConfigurationManager.AppSettings.Get("HostedAppSigningKey") : WebConfigurationManager.AppSettings.Get("ClientSecret");
        private static readonly string SecondaryClientSecret = WebConfigurationManager.AppSettings.Get("SecondaryClientSecret");
        private static readonly string Realm = WebConfigurationManager.AppSettings.Get("Realm");
        private static readonly string ServiceNamespace = WebConfigurationManager.AppSettings.Get("Realm");

        private static readonly string ClientSigningCertificatePath = WebConfigurationManager.AppSettings.Get("ClientSigningCertificatePath");
        private static readonly string ClientSigningCertificatePassword = WebConfigurationManager.AppSettings.Get("ClientSigningCertificatePassword");
        private static readonly X509Certificate2 ClientCertificate = (string.IsNullOrEmpty(ClientSigningCertificatePath) || string.IsNullOrEmpty(ClientSigningCertificatePassword)) ? null : new X509Certificate2(ClientSigningCertificatePath, ClientSigningCertificatePassword);
        private static readonly X509SigningCredentials SigningCredentials = (ClientCertificate == null) ? null : new X509SigningCredentials(ClientCertificate, Microsoft.IdentityModel.Tokens.SecurityAlgorithms.RsaSha256);

        #endregion

        #region private methods

        private static ClientContext CreateAcsClientContextForUrl(SPRemoteEventProperties properties, Uri sharepointUrl)
        {
            string contextTokenString = properties.ContextToken;

            if (String.IsNullOrEmpty(contextTokenString))
            {
                return null;
            }

            SharePointContextToken contextToken = ReadAndValidateContextToken(contextTokenString, OperationContext.Current.IncomingMessageHeaders.To.Host);
            string accessToken = GetAccessToken(contextToken, sharepointUrl.Authority).AccessToken;

            return GetClientContextWithAccessToken(sharepointUrl.ToString(), accessToken);
        }

        private static string GetAcsMetadataEndpointUrl()
        {
            return Path.Combine(GetAcsGlobalEndpointUrl(), AcsMetadataEndPointRelativeUrl);
        }

        private static string GetFormattedPrincipal(string principalName, string hostName, string realm)
        {
            if (!String.IsNullOrEmpty(hostName))
            {
                return String.Format(CultureInfo.InvariantCulture, "{0}/{1}@{2}", principalName, hostName, realm);
            }

            return String.Format(CultureInfo.InvariantCulture, "{0}@{1}", principalName, realm);
        }

        private static string GetAcsPrincipalName(string realm)
        {
            return GetFormattedPrincipal(AcsPrincipalName, new Uri(GetAcsGlobalEndpointUrl()).Host, realm);
        }

        private static string GetAcsGlobalEndpointUrl()
        {
            return String.Format(CultureInfo.InvariantCulture, "https://{0}.{1}/", GlobalEndPointPrefix, AcsHostUrl);
        }

        private static JwtSecurityTokenHandler CreateJwtSecurityTokenHandler()
        {
            return new JwtSecurityTokenHandler();
        }

        private static string GetS2SAccessTokenWithClaims(
            string targetApplicationHostName,
            string targetRealm,
            IEnumerable<Claim> claims)
        {
            return IssueToken(
                ClientId,
                IssuerId,
                targetRealm,
                SharePointPrincipal,
                targetRealm,
                targetApplicationHostName,
                true,
                claims,
                claims == null);
        }

        private static Claim[] GetClaimsWithWindowsIdentity(WindowsIdentity identity)
        {
            Claim[] claims = new Claim[]
            {
                new Claim(NameIdentifierClaimType, identity.User.Value.ToLower()),
                new Claim("nii", "urn:office:idp:activedirectory")
            };
            return claims;
        }

        private static string IssueToken(
            string sourceApplication,
            string issuerApplication,
            string sourceRealm,
            string targetApplication,
            string targetRealm,
            string targetApplicationHostName,
            bool trustedForDelegation,
            IEnumerable<Claim> claims,
            bool appOnly = false)
        {
            if (null == SigningCredentials)
            {
                throw new InvalidOperationException("SigningCredentials was not initialized");
            }

            #region Actor token

            string issuer = string.IsNullOrEmpty(sourceRealm) ? issuerApplication : string.Format("{0}@{1}", issuerApplication, sourceRealm);
            string nameid = string.IsNullOrEmpty(sourceRealm) ? sourceApplication : string.Format("{0}@{1}", sourceApplication, sourceRealm);
            string audience = string.Format("{0}/{1}@{2}", targetApplication, targetApplicationHostName, targetRealm);

            List<Claim> actorClaims = new List<Claim>();
            actorClaims.Add(new Claim(NameIdentifierClaimType, nameid));
            if (trustedForDelegation && !appOnly)
            {
                actorClaims.Add(new Claim(TrustedForImpersonationClaimType, "true"));
            }

            // Create token
            JwtSecurityToken actorToken = new JwtSecurityToken(
                issuer: issuer,
                audience: audience,
                claims: actorClaims,
                notBefore: DateTime.UtcNow,
                expires: DateTime.UtcNow.Add(HighTrustAccessTokenLifetime),
                signingCredentials: SigningCredentials
                );

            string actorTokenString = new JwtSecurityTokenHandler().WriteToken(actorToken);

            if (appOnly)
            {
                // App-only token is the same as actor token for delegated case
                return actorTokenString;
            }

            #endregion Actor token

            #region Outer token

            List<Claim> outerClaims = null == claims ? new List<Claim>() : new List<Claim>(claims);
            outerClaims.Add(new Claim(ActorTokenClaimType, actorTokenString));

            JwtSecurityToken jsonToken = new JwtSecurityToken(
                nameid, // outer token issuer should match actor token nameid
                audience,
                outerClaims,
                DateTime.UtcNow,
                DateTime.UtcNow.Add(HighTrustAccessTokenLifetime)
                );

            string accessToken = new JwtSecurityTokenHandler().WriteToken(jsonToken);

            #endregion Outer token

            return accessToken;
        }

        #endregion

        #region AcsMetadataParser

        // This class is used to get MetaData document from the global STS endpoint. It contains
        // methods to parse the MetaData document and get endpoints and STS certificate.
        public static class AcsMetadataParser
        {
            public static X509Certificate2 GetAcsSigningCert(string realm)
            {
                JsonMetadataDocument document = GetMetadataDocument(realm);

                if (null != document.keys && document.keys.Count > 0)
                {
                    JsonKey signingKey = document.keys[0];

                    if (null != signingKey && null != signingKey.keyValue)
                    {
                        return new X509Certificate2(Encoding.UTF8.GetBytes(signingKey.keyValue.value));
                    }
                }

                throw new Exception("Metadata document does not contain ACS signing certificate.");
            }

            public static string GetDelegationServiceUrl(string realm)
            {
                JsonMetadataDocument document = GetMetadataDocument(realm);

                JsonEndpoint delegationEndpoint = document.endpoints.SingleOrDefault(e => e.protocol == DelegationIssuance);

                if (null != delegationEndpoint)
                {
                    return delegationEndpoint.location;
                }
                throw new Exception("Metadata document does not contain Delegation Service endpoint Url");
            }

            private static JsonMetadataDocument GetMetadataDocument(string realm)
            {
                string acsMetadataEndpointUrlWithRealm = String.Format(CultureInfo.InvariantCulture, "{0}?realm={1}",
                                                                       GetAcsMetadataEndpointUrl(),
                                                                       realm);
                byte[] acsMetadata;
                using (WebClient webClient = new WebClient())
                {

                    acsMetadata = webClient.DownloadData(acsMetadataEndpointUrlWithRealm);
                }
                string jsonResponseString = Encoding.UTF8.GetString(acsMetadata);

                JavaScriptSerializer serializer = new JavaScriptSerializer();
                JsonMetadataDocument document = serializer.Deserialize<JsonMetadataDocument>(jsonResponseString);

                if (null == document)
                {
                    throw new Exception("No metadata document found at the global endpoint " + acsMetadataEndpointUrlWithRealm);
                }

                return document;
            }

            public static string GetStsUrl(string realm)
            {
                JsonMetadataDocument document = GetMetadataDocument(realm);

                JsonEndpoint s2sEndpoint = document.endpoints.SingleOrDefault(e => e.protocol == S2SProtocol);

                if (null != s2sEndpoint)
                {
                    return s2sEndpoint.location;
                }

                throw new Exception("Metadata document does not contain STS endpoint url");
            }

            private class JsonMetadataDocument
            {
                public string serviceName { get; set; }
                public List<JsonEndpoint> endpoints { get; set; }
                public List<JsonKey> keys { get; set; }
            }

            private class JsonEndpoint
            {
                public string location { get; set; }
                public string protocol { get; set; }
                public string usage { get; set; }
            }

            private class JsonKeyValue
            {
                public string type { get; set; }
                public string value { get; set; }
            }

            private class JsonKey
            {
                public string usage { get; set; }
                public JsonKeyValue keyValue { get; set; }
            }
        }

        #endregion
    }

    /// <summary>
    /// A JwtSecurityToken generated by SharePoint to authenticate to a 3rd party application and allow callbacks using a refresh token
    /// </summary>
    public class SharePointContextToken : JwtSecurityToken
    {
        public static SharePointContextToken Create(JwtSecurityToken contextToken)
        {
            return new SharePointContextToken(contextToken.Issuer, contextToken.Audiences.FirstOrDefault<string>(), contextToken.ValidFrom, contextToken.ValidTo, contextToken.Claims);
        }

        public SharePointContextToken(string issuer, string audience, DateTime validFrom, DateTime validTo, IEnumerable<Claim> claims)
        : base(issuer, audience, claims, validFrom, validTo)
        {
        }

        public SharePointContextToken(string issuer, string audience, DateTime validFrom, DateTime validTo, IEnumerable<Claim> claims, SecurityToken issuerToken, JwtSecurityToken actorToken)
            : base(issuer, audience, claims, validFrom, validTo, actorToken.SigningCredentials)
        {
            // This method is provided for backward compatibility with previous versions of TokenHelper.
            // Current version of JwtSecurityToken does not have a constructor that takes all the above parameters.
        }

        public SharePointContextToken(string issuer, string audience, DateTime validFrom, DateTime validTo, IEnumerable<Claim> claims, SigningCredentials signingCredentials)
            : base(issuer, audience, claims, validFrom, validTo, signingCredentials)
        {
        }

        public string NameId
        {
            get
            {
                return GetClaimValue(this, "nameid");
            }
        }

        /// <summary>
        /// The principal name portion of the context token's "appctxsender" claim
        /// </summary>
        public string TargetPrincipalName
        {
            get
            {
                string appctxsender = GetClaimValue(this, "appctxsender");

                if (appctxsender == null)
                {
                    return null;
                }

                return appctxsender.Split('@')[0];
            }
        }

        /// <summary>
        /// The context token's "refreshtoken" claim
        /// </summary>
        public string RefreshToken
        {
            get
            {
                return GetClaimValue(this, "refreshtoken");
            }
        }

        /// <summary>
        /// The context token's "CacheKey" claim
        /// </summary>
        public string CacheKey
        {
            get
            {
                string appctx = GetClaimValue(this, "appctx");
                if (appctx == null)
                {
                    return null;
                }

                ClientContext ctx = new ClientContext("http://tempuri.org");
                Dictionary<string, object> dict = (Dictionary<string, object>)ctx.ParseObjectFromJsonString(appctx);
                string cacheKey = (string)dict["CacheKey"];

                return cacheKey;
            }
        }

        /// <summary>
        /// The context token's "SecurityTokenServiceUri" claim
        /// </summary>
        public string SecurityTokenServiceUri
        {
            get
            {
                string appctx = GetClaimValue(this, "appctx");
                if (appctx == null)
                {
                    return null;
                }

                ClientContext ctx = new ClientContext("http://tempuri.org");
                Dictionary<string, object> dict = (Dictionary<string, object>)ctx.ParseObjectFromJsonString(appctx);
                string securityTokenServiceUri = (string)dict["SecurityTokenServiceUri"];

                return securityTokenServiceUri;
            }
        }

        /// <summary>
        /// The realm portion of the context token's "audience" claim
        /// </summary>
        public string Realm
        {
            get
            {
                // Get the first non-null realm
                foreach (string aud in Audiences)
                {
                    string tokenRealm = aud.Substring(aud.IndexOf('@') + 1);

                    if (string.IsNullOrEmpty(tokenRealm))
                    {
                        continue;
                    }
                    else
                    {
                        return tokenRealm;
                    }
                }

                return null;
            }
        }

        private static string GetClaimValue(JwtSecurityToken token, string claimType)
        {
            if (token == null)
            {
                throw new ArgumentNullException("token");
            }

            foreach (Claim claim in token.Claims)
            {
                if (StringComparer.Ordinal.Equals(claim.Type, claimType))
                {
                    return claim.Value;
                }
            }

            return null;
        }

    }

    /// <summary>
    /// Represents a security token which contains multiple security keys that are generated using symmetric algorithms.
    /// </summary>
    public class MultipleSymmetricKeySecurityToken : SecurityToken
    {
        /// <summary>
        /// Initializes a new instance of the MultipleSymmetricKeySecurityToken class.
        /// </summary>
        /// <param name="keys">An enumeration of Byte arrays that contain the symmetric keys.</param>
        public MultipleSymmetricKeySecurityToken(IEnumerable<byte[]> keys)
            : this(Microsoft.IdentityModel.Tokens.UniqueId.CreateUniqueId(), keys)
        {
        }

        /// <summary>
        /// Initializes a new instance of the MultipleSymmetricKeySecurityToken class.
        /// </summary>
        /// <param name="tokenId">The unique identifier of the security token.</param>
        /// <param name="keys">An enumeration of Byte arrays that contain the symmetric keys.</param>
        public MultipleSymmetricKeySecurityToken(string tokenId, IEnumerable<byte[]> keys)
        {
            if (keys == null)
            {
                throw new ArgumentNullException("keys");
            }

            if (String.IsNullOrEmpty(tokenId))
            {
                throw new ArgumentException("Value cannot be a null or empty string.", "tokenId");
            }

            foreach (byte[] key in keys)
            {
                if (key.Length <= 0)
                {
                    throw new ArgumentException("The key length must be greater then zero.", "keys");
                }
            }

            id = tokenId;
            effectiveTime = DateTime.UtcNow;
            securityKeys = CreateSymmetricSecurityKeys(keys);
        }

        /// <summary>
        /// Gets the unique identifier of the security token.
        /// </summary>
        public override string Id
        {
            get
            {
                return id;
            }
        }

        /// <summary>
        /// Gets the cryptographic keys associated with the security token.
        /// </summary>
        public override ReadOnlyCollection<SecurityKey> SecurityKeys
        {
            get
            {
                return securityKeys.AsReadOnly();
            }
        }

        /// <summary>
        /// Gets the first instant in time at which this security token is valid.
        /// </summary>
        public override DateTime ValidFrom
        {
            get
            {
                return effectiveTime;
            }
        }

        /// <summary>
        /// Gets the last instant in time at which this security token is valid.
        /// </summary>
        public override DateTime ValidTo
        {
            get
            {
                // Never expire
                return DateTime.MaxValue;
            }
        }

        /// <summary>
        /// Returns a value that indicates whether the key identifier for this instance can be resolved to the specified key identifier.
        /// </summary>
        /// <param name="keyIdentifierClause">A SecurityKeyIdentifierClause to compare to this instance</param>
        /// <returns>true if keyIdentifierClause is a SecurityKeyIdentifierClause and it has the same unique identifier as the Id property; otherwise, false.</returns>
        public override bool MatchesKeyIdentifierClause(SecurityKeyIdentifierClause keyIdentifierClause)
        {
            if (keyIdentifierClause == null)
            {
                throw new ArgumentNullException("keyIdentifierClause");
            }

            return base.MatchesKeyIdentifierClause(keyIdentifierClause);
        }

        #region private members

        private List<SecurityKey> CreateSymmetricSecurityKeys(IEnumerable<byte[]> keys)
        {
            List<SecurityKey> symmetricKeys = new List<SecurityKey>();
            foreach (byte[] key in keys)
            {
                symmetricKeys.Add(new InMemorySymmetricSecurityKey(key));
            }
            return symmetricKeys;
        }

        private string id;
        private DateTime effectiveTime;
        private List<SecurityKey> securityKeys;

        #endregion
    }

    /// <summary>
    /// Represents an OAuth response from a call to ACS server.
    /// </summary>
    public class OAuthTokenResponse
    {
        /// <summary>
        /// Default constructor.
        /// </summary>
        public OAuthTokenResponse() { }

        /// <summary>
        /// Constructs an OAuthTokenResponse object from a byte array returned from ACS server.
        /// </summary>
        /// <param name="response">The raw byte array obtained from ACS.</param>
        public OAuthTokenResponse(byte[] response)
        {
            var serializer = new JavaScriptSerializer();
            this.Data = serializer.DeserializeObject(Encoding.UTF8.GetString(response)) as Dictionary<string, object>;

            this.AccessToken = this.GetValue("access_token");
            this.TokenType = this.GetValue("token_type");
            this.Resource = this.GetValue("resource");
            this.UserType = this.GetValue("user_type");

            long epochTime = 0;
            if (long.TryParse(this.GetValue("expires_in"), out epochTime))
            {
                this.ExpiresIn = epochTime;
            }
            if (long.TryParse(this.GetValue("expires_on"), out epochTime))
            {
                this.ExpiresOn = epochTime;
            }
            if (long.TryParse(this.GetValue("not_before"), out epochTime))
            {
                this.NotBefore = epochTime;
            }
            if (long.TryParse(this.GetValue("extended_expires_in"), out epochTime))
            {
                this.ExtendedExpiresIn = epochTime;
            }
        }

        /// <summary>
        /// Gets the access token.
        /// </summary>
        public string AccessToken { get; private set; }

        /// <summary>
        /// Gets the data parsed from raw response.
        /// </summary>
        public IDictionary<string, object> Data { get; }

        /// <summary>
        /// Gets the expires in Epoch time.
        /// </summary>
        public long ExpiresIn { get; }

        /// <summary>
        /// Gets the expires on Epoch time.
        /// </summary>
        public long ExpiresOn { get; }

        /// <summary>
        /// Gets the extended expires in Epoch time.
        /// </summary>
        public long ExtendedExpiresIn { get; }

        /// <summary>
        /// Gets the expires not before Epoch time.
        /// </summary>
        public long NotBefore { get; }

        /// <summary>
        /// Gets the resource.
        /// </summary>
        public string Resource { get; }

        /// <summary>
        /// Gets the token type.
        /// </summary>
        public string TokenType { get; }

        /// <summary>
        /// Gets the user type.
        /// </summary>
        public string UserType { get; }

        /// <summary>
        /// Gets a value from the Data by the key.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <returns>The value for the key if it exists, empty string otherwise.</returns>
        private string GetValue(string key)
        {
            if (this.Data.TryGetValue(key, out object value))
            {
                return value as string;
            }
            else
            {
                return string.Empty;
            }
        }
    }

    /// <summary>
    /// Represents a Web client for making an OAuth call to ACS server.
    /// </summary>
    public class OAuthClient
    {
        /// <summary>
        /// Gets an OAuthTokenResponse with a refresh token.
        /// </summary>
        /// <param name="uri">The Uri of the ACS server.</param>
        /// <param name="clientId">Client ID.</param>
        /// <param name="ClientSecret">Client secret.</param>
        /// <param name="refreshToken">Refresh token.</param>
        /// <param name="resource">Resource.</param>
        /// <returns>Response from ACS server.</returns>
        public static OAuthTokenResponse GetAccessTokenWithRefreshToken(string uri, string clientId,
            string ClientSecret, string refreshToken, string resource)
        {
            WebClient client = new WebClient();
            NameValueCollection values = new NameValueCollection
            {
                { "grant_type", "refresh_token" },
                { "client_id", clientId },
                { "client_secret", ClientSecret },
                { "refresh_token", refreshToken },
                { "resource", resource }
            };

            byte[] response = client.UploadValues(uri, "POST", values);

            return new OAuthTokenResponse(response);
        }

        /// <summary>
        /// Gets an OAuthTokenResponse with client credentials.
        /// </summary>
        /// <param name="uri">The Uri of the ACS server.</param>
        /// <param name="clientId">Client ID.</param>
        /// <param name="ClientSecret">Client secret.</param>
        /// <param name="resource">Resource.</param>
        /// <returns>Response from ACS server.</returns>
        public static OAuthTokenResponse GetAccessTokenWithClientCredentials(string uri, string clientId,
            string ClientSecret, string resource)
        {
            WebClient client = new WebClient();
            NameValueCollection values = new NameValueCollection
            {
                { "grant_type", "client_credentials" },
                { "client_id", clientId },
                { "client_secret", ClientSecret },
                { "resource", resource }
            };

            byte[] response = client.UploadValues(uri, "POST", values);

            return new OAuthTokenResponse(response);
        }

        /// <summary>
        /// Gets an OAuthTokenResponse with an authorization code.
        /// </summary>
        /// <param name="uri">The Uri of the ACS server.</param>
        /// <param name="clientId">Client ID.</param>
        /// <param name="ClientSecret">Client secret.</param>
        /// <param name="authorizationCode">Authorization code.</param>
        /// <param name="redirectUri">Redirect Uri.</param>
        /// <param name="resource">Resource.</param>
        /// <returns>Response from ACS server.</returns>
        public static OAuthTokenResponse GetAccessTokenWithAuthorizationCode(string uri, string clientId,
            string ClientSecret, string authorizationCode, string redirectUri, string resource)
        {
            WebClient client = new WebClient();
            NameValueCollection values = new NameValueCollection
            {
                { "grant_type", "authorization_code" },
                { "client_id", clientId },
                { "client_secret", ClientSecret },
                { "code", authorizationCode },
                { "redirect_uri", redirectUri },
                { "resource", resource }
            };

            byte[] response = client.UploadValues(uri, "POST", values);

            return new OAuthTokenResponse(response);
        }
    }
}
