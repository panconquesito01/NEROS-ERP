using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.Localization;
using Microsoft.Extensions.Localization;
using Neros.Blazor.Servicios;

namespace Neros.Blazor.Localizacion;

public static class ConfiguracionLocalizacion
{
    public static IServiceCollection AgregarLocalizacionNeros(this IServiceCollection services)
    {
        services.AddLocalization();
        services.Configure<RequestLocalizationOptions>(opciones =>
        {
            opciones.SetDefaultCulture(Idiomas.Predeterminado)
                .AddSupportedCultures(Idiomas.Codigos)
                .AddSupportedUICultures(Idiomas.Codigos);
            opciones.ApplyCurrentCultureToResponseHeaders = true;
        });
        return services;
    }

    public static void MapearIdioma(this WebApplication app) =>
        app.MapPost("/idioma", CambiarAsync).AllowAnonymous();

    private static async Task<IResult> CambiarAsync(HttpContext context, IAntiforgery antiforgery, IStringLocalizer<TextosComunes> textos)
    {
        try
        {
            await antiforgery.ValidateRequestAsync(context);
        }
        catch (AntiforgeryValidationException)
        {
            return Results.BadRequest(textos["Solicitud.Vencida"].Value);
        }
        var form = await context.Request.ReadFormAsync(context.RequestAborted);
        var cultura = form["Cultura"].ToString();
        if (Idiomas.EsCompatible(cultura))
        {
            context.Response.Cookies.Append(
                CookieRequestCultureProvider.DefaultCookieName,
                CookieRequestCultureProvider.MakeCookieValue(new RequestCulture(cultura.ToLowerInvariant())),
                new CookieOptions
                {
                    Expires = DateTimeOffset.UtcNow.AddYears(1), IsEssential = true, HttpOnly = true,
                    SameSite = SameSiteMode.Lax, Secure = context.Request.IsHttps
                });
        }
        return Results.LocalRedirect(RutaLocal.Validar(form["Volver"]) ?? "/");
    }
}
