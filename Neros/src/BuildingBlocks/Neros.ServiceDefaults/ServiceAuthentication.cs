using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;

namespace Neros.ServiceDefaults;

public static class ServiceAuthentication
{
    public static IServiceCollection AddNerosServiceAuthentication(this IServiceCollection services, Uri authority, string audience)
    {
        if (!authority.IsAbsoluteUri || authority.Scheme != Uri.UriSchemeHttps || !string.IsNullOrEmpty(authority.UserInfo))
            throw new ArgumentException("La autoridad de identidad requiere HTTPS sin credenciales en URL.", nameof(authority));
        ArgumentException.ThrowIfNullOrWhiteSpace(audience);
        services.AddSingleton(TimeProvider.System);
        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme).AddJwtBearer(options =>
        {
            options.Authority = authority.AbsoluteUri;
            options.Audience = audience;
            options.RequireHttpsMetadata = true;
            options.MapInboundClaims = false;
            options.IncludeErrorDetails = false;
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateIssuerSigningKey = true,
                ValidateLifetime = true,
                RequireSignedTokens = true,
                RequireExpirationTime = true,
                ClockSkew = TimeSpan.Zero,
                ValidAlgorithms = [SecurityAlgorithms.RsaSha256, SecurityAlgorithms.EcdsaSha256]
            };
        });
        services.AddAuthorization(options => options.FallbackPolicy = new AuthorizationPolicyBuilder()
            .RequireAuthenticatedUser().Build());
        services.AddSingleton<IAuthorizationHandler, ServicePermissionHandler>();
        return services;
    }

    public static AuthorizationPolicyBuilder RequireNerosPermission(this AuthorizationPolicyBuilder builder, string scope, string permission)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(scope);
        ArgumentException.ThrowIfNullOrWhiteSpace(permission);
        return builder.RequireAuthenticatedUser().AddRequirements(new ServicePermission(scope, permission));
    }

    private sealed record ServicePermission(string Scope, string Permission) : IAuthorizationRequirement;

    private sealed class ServicePermissionHandler(TimeProvider clock) : AuthorizationHandler<ServicePermission>
    {
        protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, ServicePermission requirement)
        {
            if (SecurityContext.TryFromAuthenticatedPrincipal(context.User, clock, out var security)
                && security!.Permissions.Contains(requirement.Permission)
                && context.User.FindAll("scope").SelectMany(claim => claim.Value.Split(' ', StringSplitOptions.RemoveEmptyEntries))
                    .Contains(requirement.Scope, StringComparer.Ordinal))
            {
                context.Succeed(requirement);
            }
            return Task.CompletedTask;
        }
    }
}