using System.Net;
using System.Net.Http.Headers;
using Neros.Contracts.Autenticacion;

namespace Neros.Blazor.Servicios;

public sealed class ClienteNeros(HttpClient http)
{
    public async Task<AccesoConcedido?> IniciarAsync(SolicitudAcceso solicitud, CancellationToken cancellationToken)
    {
        using var respuesta = await http.PostAsJsonAsync("api/acceso/login", solicitud, cancellationToken);
        if (respuesta.StatusCode == HttpStatusCode.Unauthorized)
        {
            return null;
        }
        respuesta.EnsureSuccessStatusCode();
        return await respuesta.Content.ReadFromJsonAsync<AccesoConcedido>(cancellationToken);
    }

    public Task<UsuarioActual?> ConsultarUsuarioAsync(string token, CancellationToken cancellationToken) =>
        ConsultarAsync<UsuarioActual>("api/acceso/yo", token, cancellationToken);

    public async Task<IReadOnlyList<EmpresaDisponible>> EmpresasAsync(string token, CancellationToken cancellationToken) =>
        await ConsultarAsync<List<EmpresaDisponible>>("api/empresas", token, cancellationToken) ?? [];

    public Task<InicioEmpresa?> InicioAsync(string token, Guid empresaId, CancellationToken cancellationToken) =>
        ConsultarAsync<InicioEmpresa>($"api/empresas/{empresaId}/inicio", token, cancellationToken);

    public async Task<EmpresaDisponible?> SeleccionarAsync(string token, Guid empresaId, CancellationToken cancellationToken)
    {
        using var peticion = CrearPeticion(HttpMethod.Post, $"api/empresas/{empresaId}/seleccionar", token);
        using var respuesta = await http.SendAsync(peticion, cancellationToken);
        if (respuesta.StatusCode == HttpStatusCode.NotFound)
        {
            return null;
        }
        respuesta.EnsureSuccessStatusCode();
        return await respuesta.Content.ReadFromJsonAsync<EmpresaDisponible>(cancellationToken);
    }

    public async Task CerrarAsync(string token, CancellationToken cancellationToken)
    {
        using var peticion = CrearPeticion(HttpMethod.Post, "api/acceso/logout", token);
        using var respuesta = await http.SendAsync(peticion, cancellationToken);
        if (respuesta.StatusCode != HttpStatusCode.Unauthorized)
        {
            respuesta.EnsureSuccessStatusCode();
        }
    }

    private async Task<T?> ConsultarAsync<T>(string ruta, string token, CancellationToken cancellationToken)
    {
        using var peticion = CrearPeticion(HttpMethod.Get, ruta, token);
        using var respuesta = await http.SendAsync(peticion, cancellationToken);
        if (respuesta.StatusCode == HttpStatusCode.NotFound)
        {
            return default;
        }
        respuesta.EnsureSuccessStatusCode();
        return await respuesta.Content.ReadFromJsonAsync<T>(cancellationToken);
    }

    private static HttpRequestMessage CrearPeticion(HttpMethod metodo, string ruta, string token) => new(metodo, ruta)
    {
        Headers = { Authorization = new AuthenticationHeaderValue("Bearer", token) }
    };
}