using System.Text.RegularExpressions;
using Microsoft.Playwright;

namespace Neros.Tests;

internal static partial class NavegacionPruebas
{
    [GeneratedRegex("legal/aceptar|empresas|cuenta/clave", RegexOptions.IgnoreCase)]
    internal static partial Regex DestinoTrasLogin();

    public static async Task EntrarAsync(IPage page, string correo, string clave)
    {
        await page.GotoAsync("/login");
        await page.GetByLabel("Correo electrónico", new() { Exact = true }).FillAsync(correo);
        await page.GetByLabel("Contraseña", new() { Exact = true }).FillAsync(clave);
        await page.GetByRole(AriaRole.Button, new() { Name = "Entrar a Neros" }).ClickAsync();
        await page.WaitForURLAsync(DestinoTrasLogin(), new() { Timeout = 30_000 });
        await CompletarLegalesSiPendientesAsync(page);
    }

    public static async Task CompletarLegalesSiPendientesAsync(IPage page)
    {
        if (!page.Url.Contains("/legal/aceptar", StringComparison.OrdinalIgnoreCase))
        {
            return;
        }
        await page.WaitForSelectorAsync("input[data-legal-check]", new() { Timeout = 30_000 });
        foreach (var casilla in await page.Locator("input[data-legal-check]").AllAsync())
        {
            await casilla.CheckAsync();
        }
        await page.Locator("form[action='/sesion/legal/aceptar'] button[type='submit']").ClickAsync();
        await page.WaitForURLAsync("**/empresas", new() { Timeout = 30_000 });
    }
}
