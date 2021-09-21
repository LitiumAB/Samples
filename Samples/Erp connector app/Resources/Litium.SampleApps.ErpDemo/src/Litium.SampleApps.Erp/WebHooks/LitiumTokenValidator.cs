using IdentityModel;
using IdentityModel.Client;
using Litium.SampleApps.Erp.LitiumClients;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Security.Cryptography;

namespace Litium.SampleApps.Erp.WebHooks
{
    public class LitiumTokenValidator
    {
        private Lazy<TokenValidationParameters> _tokenValidationParameters;

        public LitiumTokenValidator()
        {
            _tokenValidationParameters = new Lazy<TokenValidationParameters>(() =>
            {
                var client = new HttpClient();
                var doc = client.GetDiscoveryDocumentAsync(new DiscoveryDocumentRequest
                {
                    Address = LitiumSettings.Host,
                    Policy =
                    {
                        ValidateIssuerName = false
                    }
                }).GetAwaiter().GetResult();

                var keys = new List<SecurityKey>();
                foreach (var webKey in doc.KeySet.Keys)
                {
                    var e = Base64Url.Decode(webKey.E);
                    var n = Base64Url.Decode(webKey.N);

                    var key = new RsaSecurityKey(new RSAParameters { Exponent = e, Modulus = n })
                    {
                        KeyId = webKey.Kid
                    };

                    keys.Add(key);
                }

                return new TokenValidationParameters
                {
                    ValidateAudience = false,
                    ValidIssuer = doc.Issuer,
                    IssuerSigningKeys = keys,

                    NameClaimType = JwtClaimTypes.Name,
                    RoleClaimType = JwtClaimTypes.Role,

                    RequireSignedTokens = true
                };

            });
        }

        public bool Validate(HttpRequestMessage request)
        {
            if (request.Headers.Authorization == null
                   || request.Headers.Authorization.Scheme != "Bearer"
                   || string.IsNullOrEmpty(request.Headers.Authorization.Parameter))
            {
                return false;
            }

            try
            {
                var handler = new System.IdentityModel.Tokens.Jwt.JwtSecurityTokenHandler();
                handler.InboundClaimTypeMap.Clear();

                var claims = handler.ValidateToken(request.Headers.Authorization.Parameter, _tokenValidationParameters.Value, out var _);
                if (claims?.Identity?.Name != LitiumSettings.ClientId)
                {
                    return false;
                }
            }
            catch (Exception)
            {
                return false;
            }

            return true;
        }
    }
}
