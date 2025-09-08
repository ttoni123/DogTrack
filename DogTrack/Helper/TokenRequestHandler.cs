using DogTrack.DataAccess.IDogTrackDataAccess;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using Microsoft.IdentityModel.Tokens;
using OpenIddict.Abstractions;
using OpenIddict.Server;
using OpenIddict.Validation.AspNetCore;
using System.Security.Claims;

namespace DogTrack.Helper
{
    public class TokenRequestHandler : IOpenIddictServerHandler<OpenIddictServerEvents.ValidateTokenRequestContext>
    {
        public ValueTask HandleAsync(OpenIddictServerEvents.ValidateTokenRequestContext context)
        {
            // reject token requests that don't use grant_type=client_credentials.
            if (!context.Request.IsPasswordGrantType())
            {
                context.Reject
                (
                    error: OpenIddictConstants.Errors.UnsupportedGrantType,
                    description: "Auth_InvalidGrantType"
                );
            }

            return default;
        }
    }

    public class HandleTokenRequestHandler : IOpenIddictServerHandler<OpenIddictServerEvents.HandleTokenRequestContext>
    {
        private readonly IDogTrackDataAccess dataAccess;

        public HandleTokenRequestHandler(IDogTrackDataAccess dataAccess)
        {
            this.dataAccess = dataAccess;
        }

        public ValueTask HandleAsync(OpenIddictServerEvents.HandleTokenRequestContext context)
        {
            // basic input validation
            if (string.IsNullOrEmpty(context.Request.Username) || string.IsNullOrEmpty(context.Request.Password))
            {
                context.Reject
                (
                    error: OpenIddictConstants.Errors.InvalidRequest
                );

                return default;
            }

            try
            {
                var user = dataAccess.Login(context.Request.Username, context.Request.Password).GetAwaiter().GetResult();

                if (user == 0)
                {
                    context.Reject(error: OpenIddictConstants.Errors.AccessDenied, description: "Invalid client credentials");
                    return default;

                }
                string sessionNonce = Guid.NewGuid().ToString("N");

                SetPrincipal(context, user.ToString(), sessionNonce);

                return default;
            }
            catch
            {
                // generic handler for all  errors that might occur
                context.Reject(error: OpenIddictConstants.Errors.ServerError, description: "Error calling authentification service. Please contact IT support.");
                return default;
            }
        }

        internal void SetPrincipal(OpenIddictServerEvents.HandleTokenRequestContext context, string userId, string sessionNonce)
        {
            var identity = new ClaimsIdentity
            (
                [
                    new Claim(OpenIddictConstants.Claims.Subject, userId),
                    new Claim(OpenIddictConstants.Claims.Role, userId),
                    new Claim("UserId", userId),
                    new Claim("SessionNonce", sessionNonce)
                ],
                OpenIddictValidationAspNetCoreDefaults.AuthenticationScheme
            );

            foreach (var claim in identity.Claims)
            {
                if (claim.Type == "SessionNonce")
                {
                    claim.SetDestinations(OpenIdConnectParameterNames.RefreshToken);
                }
                else
                {
                    claim.SetDestinations(OpenIdConnectParameterNames.AccessToken);
                }
            }

            context.Principal = new ClaimsPrincipal(identity);
            context.Principal.SetScopes
            (
                new[]
                {
                    OpenIddictConstants.Scopes.OpenId
                }
            );
        }
    }

    public class ApplyTokenResponseHandler : IOpenIddictServerHandler<OpenIddictServerEvents.ApplyTokenResponseContext>
    {
        public ValueTask HandleAsync(OpenIddictServerEvents.ApplyTokenResponseContext context)
        {
            return default;
        }
    }
}
