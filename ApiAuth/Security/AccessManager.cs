using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Principal;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using ApiAuth.Models;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Distributed;
using Newtonsoft.Json;
using ApiAuth.Models.AppSettings;
using Microsoft.Extensions.Options;
using ApiAuth.AppConfig.Extensions;

namespace ApiAuth.Security
{
    public class AccessManager
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly AppSettings _appSettings;
        private readonly IDistributedCache _cache;

        public AccessManager(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            IOptions<AppSettings> appSettings,
            IDistributedCache cache)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _appSettings = appSettings.Value;
            _cache = cache;
        }

        public async Task<bool> ValidateCredentials(AccessCredentials credentials)
        {
            bool validCredentials = false;

            if (credentials != null && !string.IsNullOrWhiteSpace(credentials.UserID))
            {
                switch (credentials.GrantType)
                {
                    case GrantType.PASSWORD:
                        // Validate if user exists in Identity
                        var userIdentity = await _userManager.FindByNameAsync(credentials.UserID);

                        if (userIdentity != null)
                        {
                            // Logon by credentials
                            var result = await _signInManager.CheckPasswordSignInAsync(userIdentity, credentials.Password, false);
                            if (result.Succeeded)
                            {
                                // Verify if user is in role
                                validCredentials = await _userManager.IsInRoleAsync(userIdentity, Roles.ROLE_API_AUTH);
                            }
                        }
                        break;
                    case GrantType.REFRESH_TOKEN:
                        if (!string.IsNullOrWhiteSpace(credentials.RefreshToken))
                        {
                            RefreshTokenData refreshTokenBase = null;

                            string storedCacheToken = await _cache.GetStringAsync(credentials.RefreshToken);

                            if (!string.IsNullOrWhiteSpace(storedCacheToken))
                            {
                                refreshTokenBase = JsonConvert.DeserializeObject<RefreshTokenData>(storedCacheToken);
                            }

                            validCredentials = (refreshTokenBase != null &&
                                credentials.UserID == refreshTokenBase.UserID &&
                                credentials.RefreshToken == refreshTokenBase.RefreshToken);

                            // Clear refresh token to renew
                            if (validCredentials)
                                _cache.Remove(credentials.RefreshToken);
                        }
                        break;
                    default:
                        throw new NotImplementedException();
                }
            }

            return validCredentials;
        }

        public Token GenerateToken(AccessCredentials credentials)
        {
            // TODO: need to set by database user configs
            ClaimsIdentity identity = new(
                new GenericIdentity(credentials.UserID, "Login"),
                new[] {
                        new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString("N")),
                        new Claim(JwtRegisteredClaimNames.UniqueName, credentials.UserID),
                        new Claim(ClaimTypes.Role, Roles.ROLE_API_AUTH)
                }
            );

            DateTime createdAt = DateTime.Now;
            DateTime expirationDate = createdAt + TimeSpan.FromSeconds(_appSettings.TokenConfigurations.TokenExpirationSeconds);

            var handler = new JwtSecurityTokenHandler();
            var securityToken = handler.CreateToken(new SecurityTokenDescriptor
            {
                Issuer = _appSettings.TokenConfigurations.Issuer,
                Audience = _appSettings.TokenConfigurations.Audience,
                SigningCredentials = new SigningCredentials(
                        new SymmetricSecurityKey(JwtSecurityConfig.GetBytesFromKey(_appSettings.TokenConfigurations.Secret)),
                        SecurityAlgorithms.HmacSha256Signature),
                Subject = identity,
                NotBefore = createdAt,
                Expires = expirationDate
            });

            var tokenString = handler.WriteToken(securityToken);

            var token = new Token()
            {
                Authenticated = true,
                Created = createdAt.ToString("yyyy-MM-dd HH:mm:ss"),
                Expiration = expirationDate.ToString("yyyy-MM-dd HH:mm:ss"),
                AccessToken = tokenString,
                RefreshToken = Guid.NewGuid().ToString().Replace("-", string.Empty),
                Message = "OK"
            };

            // Data to stores the refresh token in cache
            var refreshTokenData = new RefreshTokenData
            {
                RefreshToken = token.RefreshToken,
                UserID = credentials.UserID
            };

            // Timeset to expiration (managed by cache distribution [InMem|Redis])
            TimeSpan refreshTokenExpiration = TimeSpan.FromSeconds(_appSettings.TokenConfigurations.RefreshTokenExpirationSeconds);

            DistributedCacheEntryOptions cacheOptions = new();

            cacheOptions.SetAbsoluteExpiration(refreshTokenExpiration);

            _cache.SetString(token.RefreshToken,
                JsonConvert.SerializeObject(refreshTokenData),
                cacheOptions);

            return token;
        }
    }
}