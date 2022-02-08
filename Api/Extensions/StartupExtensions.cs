using Api.Options;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;

namespace Api.Extensions
{
    public static class StartupExtensions
    {
        public static AuthenticationBuilder AddJwtAuthentication(this IServiceCollection services, AuthOptions authOptions)
        {
            return services.AddAuthentication(x =>
                {
                    x.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                    x.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
                })
                .AddJwtBearer(x =>
                {
                    x.SaveToken = true;
                    x.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuer = true,
                        ValidateAudience = true,
                        ValidateLifetime = true,
                        ValidateIssuerSigningKey = true,
                        ValidIssuer = authOptions.Issuer,
                        ValidAudience = authOptions.Audience,
                        IssuerSigningKey = authOptions.GetSymmetricSecurityKey(),
                    };
                });
        }

        public static AuthOptions AddAuthOptions(this IServiceCollection services, IConfiguration configuration)
        {
            IConfigurationSection authOptionsSection = configuration.GetSection("Auth");
            
            AuthOptions authOptions = new AuthOptions();
            authOptionsSection.Bind(authOptions);
            
            services.Configure<AuthOptions>(authOptionsSection);

            return authOptions;
        }
    }
}