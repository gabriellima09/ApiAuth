using System;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.DependencyInjection;
using ApiAuth.Models.AppSettings;
using System.Text;
using Microsoft.IdentityModel.Tokens;

namespace ApiAuth.AppConfig.Extensions
{
    public static class JwtSecurityConfig
    {
        private static AppSettings AppSettings;

        public static IServiceCollection AddJwtSecurity(
            this IServiceCollection services,
            AppSettings appSettings)
        {
            AppSettings = appSettings;

            services.AddAuthentication(authOptions =>
            {
                authOptions.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                authOptions.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            }).AddJwtBearer(bearerOptions =>
            {
                var paramsValidation = bearerOptions.TokenValidationParameters;
                paramsValidation.IssuerSigningKey = new SymmetricSecurityKey(GetBytesFromKey(AppSettings.TokenConfigurations.Secret));
                paramsValidation.ValidAudience = appSettings.TokenConfigurations.Audience;
                paramsValidation.ValidIssuer = appSettings.TokenConfigurations.Issuer;
                paramsValidation.ValidateIssuerSigningKey = true;
                paramsValidation.ValidateLifetime = true;

                // Used for sync delay of expiration tokens
                paramsValidation.ClockSkew = TimeSpan.Zero;
            });

            services.AddAuthorization(auth =>
            {
                auth.AddPolicy("Bearer", new AuthorizationPolicyBuilder()
                    .AddAuthenticationSchemes(JwtBearerDefaults.AuthenticationScheme‌​)
                    .RequireAuthenticatedUser().Build());
            });

            return services;
        }

        public static byte[] GetBytesFromKey(string key)
        {
            return Encoding.ASCII.GetBytes(key);
        }
    }
}