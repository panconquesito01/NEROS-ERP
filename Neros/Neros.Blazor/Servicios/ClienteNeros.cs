using System.Net;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Text.Json;
using Neros.Contracts.Administracion;
using Neros.Contracts.Autenticacion;
using Neros.Contracts.Cuenta;
using Neros.Contracts.Globalizacion;
using Neros.Contracts.Operacion;
using Neros.Contracts.Organizacion;
using Neros.Contracts.Privacidad;

namespace Neros.Blazor.Servicios;

public sealed class ClienteNeros(HttpClient http)
{
    public async Task<IReadOnlyList<DefinicionCookiePublica>> CookiesAsync(CancellationToken cancellationToken) =>
        await http.GetFromJsonAsync<List<DefinicionCookiePublica>>("api/privacidad/cookies", cancellationToken) ?? [];

    public async Task<IReadOnlyList<DocumentoLegalPendiente>> PendientesLegalesAsync(string token, CancellationToken cancellationToken) =>
        await ConsultarAsync<List<DocumentoLegalPendiente>>("api/privacidad/documentos/pendientes", token, cancellationToken) ?? [];

    public async Task<DocumentoLegalPublicado?> DocumentoLegalAsync(string token, string codigo, CancellationToken cancellationToken) =>
        await ConsultarAsync<DocumentoLegalPublicado>($"api/privacidad/documentos/{Uri.EscapeDataString(codigo)}", token, cancellationToken);

    public async Task<bool> AceptarLegalAsync(string token, string codigo, SolicitudAceptacionLegal solicitud, CancellationToken cancellationToken)
    {
        using var peticion = CrearPeticion(HttpMethod.Post, $"api/privacidad/documentos/{Uri.EscapeDataString(codigo)}/aceptar", token);
        peticion.Content = JsonContent.Create(solicitud);
        using var respuesta = await http.SendAsync(peticion, cancellationToken);
        return respuesta.IsSuccessStatusCode;
    }

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

    public Task<PaginaProgramaOperativo?> ProgramaOperativoAsync(
        string token, Guid empresaId, string clavePrograma, int pagina, CancellationToken cancellationToken) =>
        ConsultarAsync<PaginaProgramaOperativo>(
            $"api/empresas/{empresaId}/programas/{Uri.EscapeDataString(clavePrograma)}?pagina={pagina}",
            token, cancellationToken);

    public async Task<(BorradorProgramaCreado? Creado, bool ModuloNoDisponible)> CrearBorradorProgramaAsync(
        string token, Guid empresaId, string clavePrograma, CancellationToken cancellationToken)
    {
        using var peticion = CrearPeticion(HttpMethod.Post,
            $"api/empresas/{empresaId}/programas/{Uri.EscapeDataString(clavePrograma)}/borrador", token, empresaId);
        using var respuesta = await http.SendAsync(peticion, cancellationToken);
        if (respuesta.StatusCode == HttpStatusCode.ServiceUnavailable)
            return (null, true);
        if (respuesta.StatusCode == HttpStatusCode.Forbidden)
            return (null, false);
        respuesta.EnsureSuccessStatusCode();
        var creado = await respuesta.Content.ReadFromJsonAsync<BorradorProgramaCreado>(cancellationToken);
        return (creado, false);
    }

    public Task<DetalleProgramaOperativo?> DetalleProgramaAsync(
        string token, Guid empresaId, string clavePrograma, Guid id, CancellationToken cancellationToken) =>
        ConsultarAsync<DetalleProgramaOperativo>(
            $"api/empresas/{empresaId}/programas/{Uri.EscapeDataString(clavePrograma)}/{id:D}",
            token, cancellationToken);

    public async Task<bool> GuardarProgramaAsync(
        string token, Guid empresaId, string clavePrograma, Guid id, SolicitudGuardarPrograma solicitud, CancellationToken cancellationToken)
    {
        using var peticion = CrearPeticion(HttpMethod.Put,
            $"api/empresas/{empresaId}/programas/{Uri.EscapeDataString(clavePrograma)}/{id:D}", token, empresaId);
        peticion.Content = JsonContent.Create(solicitud);
        using var respuesta = await http.SendAsync(peticion, cancellationToken);
        return respuesta.IsSuccessStatusCode;
    }

    public async Task<LineaProgramaOperativo?> AgregarLineaProgramaAsync(
        string token, Guid empresaId, string clavePrograma, Guid id, SolicitudLineaPrograma solicitud, CancellationToken cancellationToken)
    {
        using var peticion = CrearPeticion(HttpMethod.Post,
            $"api/empresas/{empresaId}/programas/{Uri.EscapeDataString(clavePrograma)}/{id:D}/lineas", token, empresaId);
        peticion.Content = JsonContent.Create(solicitud);
        using var respuesta = await http.SendAsync(peticion, cancellationToken);
        if (!respuesta.IsSuccessStatusCode) return null;
        return await respuesta.Content.ReadFromJsonAsync<LineaProgramaOperativo>(cancellationToken);
    }

    public async Task<bool> AccionProgramaAsync(
        string token, Guid empresaId, string clavePrograma, Guid id, string accion, CancellationToken cancellationToken)
    {
        using var peticion = CrearPeticion(HttpMethod.Post,
            $"api/empresas/{empresaId}/programas/{Uri.EscapeDataString(clavePrograma)}/{id:D}/acciones/{Uri.EscapeDataString(accion)}", token, empresaId);
        using var respuesta = await http.SendAsync(peticion, cancellationToken);
        return respuesta.IsSuccessStatusCode;
    }

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

    public async Task<(AccesoConcedido? Acceso, string? Error)> CambiarClaveAsync(string token, SolicitudCambioClave solicitud, CancellationToken cancellationToken)
    {
        using var peticion = CrearPeticion(HttpMethod.Post, "api/cuenta/clave", token);
        peticion.Content = JsonContent.Create(solicitud);
        using var respuesta = await http.SendAsync(peticion, cancellationToken);
        if (respuesta.StatusCode == HttpStatusCode.BadRequest)
        {
            return (null, await CodigoErrorAsync(respuesta, cancellationToken) ?? "datos");
        }
        respuesta.EnsureSuccessStatusCode();
        return (await respuesta.Content.ReadFromJsonAsync<AccesoConcedido>(cancellationToken), null);
    }

    public async Task<IReadOnlyList<SesionActiva>> SesionesAsync(string token, CancellationToken cancellationToken) =>
        await ConsultarAsync<List<SesionActiva>>("api/cuenta/sesiones", token, cancellationToken) ?? [];

    public async Task<bool> CerrarSesionAsync(string token, Guid sesionId, CancellationToken cancellationToken)
    {
        using var peticion = CrearPeticion(HttpMethod.Post, $"api/cuenta/sesiones/{sesionId}/cerrar", token);
        using var respuesta = await http.SendAsync(peticion, cancellationToken);
        if (respuesta.StatusCode == HttpStatusCode.NotFound)
        {
            return false;
        }
        respuesta.EnsureSuccessStatusCode();
        return true;
    }

    public async Task<int> CerrarSesionesAsync(string token, bool incluirActual, CancellationToken cancellationToken)
    {
        using var peticion = CrearPeticion(HttpMethod.Post, incluirActual ? "api/cuenta/sesiones/cerrar-todas" : "api/cuenta/sesiones/cerrar-otras", token);
        using var respuesta = await http.SendAsync(peticion, cancellationToken);
        respuesta.EnsureSuccessStatusCode();
        return (await respuesta.Content.ReadFromJsonAsync<SesionesCerradas>(cancellationToken))?.Cantidad ?? 0;
    }

    public async Task<PaginaUsuarios> UsuariosAsync(string token, string? buscar, int pagina, int tamano, CancellationToken cancellationToken)
    {
        var ruta = $"api/admin/usuarios?pagina={pagina}&tamano={tamano}";
        if (!string.IsNullOrWhiteSpace(buscar)) ruta += $"&buscar={Uri.EscapeDataString(buscar)}";
        return await ConsultarAsync<PaginaUsuarios>(ruta, token, cancellationToken) ?? new PaginaUsuarios([], 0, pagina, tamano);
    }

    public async Task<PaginaEmpresas> EmpresasPlataformaAsync(string token, string? buscar, int pagina, int tamano, CancellationToken cancellationToken)
    {
        var ruta = $"api/admin/plataforma/empresas?pagina={pagina}&tamano={tamano}";
        if (!string.IsNullOrWhiteSpace(buscar)) ruta += $"&buscar={Uri.EscapeDataString(buscar)}";
        return await ConsultarAsync<PaginaEmpresas>(ruta, token, cancellationToken) ?? new PaginaEmpresas([], 0, pagina, tamano);
    }

    public async Task<(EmpresaAdministrada? Empresa, string? Error)> CrearEmpresaPlataformaAsync(
        string token, SolicitudCrearEmpresa solicitud, CancellationToken cancellationToken)
    {
        using var peticion = CrearPeticion(HttpMethod.Post, "api/admin/plataforma/empresas", token);
        peticion.Content = JsonContent.Create(solicitud);
        using var respuesta = await http.SendAsync(peticion, cancellationToken);
        if (respuesta.StatusCode == HttpStatusCode.Conflict)
            return (null, CodigosAdministracion.EmpresaDuplicada);
        respuesta.EnsureSuccessStatusCode();
        return (await respuesta.Content.ReadFromJsonAsync<EmpresaAdministrada>(cancellationToken), null);
    }

    public async Task<(byte[]? Contenido, string? ContentType)> LogoEmpresaAsync(
        string token, Guid empresaId, CancellationToken cancellationToken)
    {
        using var peticion = CrearPeticion(HttpMethod.Get, $"api/empresas/{empresaId:D}/logo", token);
        using var respuesta = await http.SendAsync(peticion, cancellationToken);
        if (respuesta.StatusCode == HttpStatusCode.NotFound) return (null, null);
        respuesta.EnsureSuccessStatusCode();
        return (await respuesta.Content.ReadAsByteArrayAsync(cancellationToken),
            respuesta.Content.Headers.ContentType?.MediaType);
    }

    public async Task<bool> SubirLogoEmpresaAsync(string token, Guid empresaId, Stream archivo, string contentType, CancellationToken cancellationToken)
    {
        using var peticion = CrearPeticion(HttpMethod.Put, $"api/admin/plataforma/empresas/{empresaId}/logo", token);
        using var contenido = new MultipartFormDataContent();
        contenido.Add(new StreamContent(archivo), "archivo", "logo");
        peticion.Content = contenido;
        using var respuesta = await http.SendAsync(peticion, cancellationToken);
        return respuesta.IsSuccessStatusCode;
    }

    public async Task<PaginaMiembrosEmpresa> MiembrosEmpresaAsync(
        string token, Guid empresaId, string? buscar, int pagina, int tamano, CancellationToken cancellationToken)
    {
        var ruta = $"api/admin/empresa/miembros?pagina={pagina}&tamano={tamano}";
        if (!string.IsNullOrWhiteSpace(buscar)) ruta += $"&buscar={Uri.EscapeDataString(buscar)}";
        using var peticion = CrearPeticion(HttpMethod.Get, ruta, token, empresaId);
        using var respuesta = await http.SendAsync(peticion, cancellationToken);
        respuesta.EnsureSuccessStatusCode();
        return await respuesta.Content.ReadFromJsonAsync<PaginaMiembrosEmpresa>(cancellationToken)
               ?? new PaginaMiembrosEmpresa([], 0, pagina, tamano);
    }

    public async Task<(string? Clave, string? Error)> RestablecerClaveEmpresaAsync(
        string token, Guid empresaId, string usuarioId, CancellationToken cancellationToken)
    {
        using var peticion = CrearPeticion(HttpMethod.Post, $"api/admin/empresa/miembros/{Uri.EscapeDataString(usuarioId)}/restablecer-clave", token, empresaId);
        using var respuesta = await http.SendAsync(peticion, cancellationToken);
        if (respuesta.StatusCode == HttpStatusCode.NotFound) return (null, "no_encontrado");
        if (respuesta.StatusCode == HttpStatusCode.BadRequest) return (null, CodigosCuenta.PropiaCuenta);
        respuesta.EnsureSuccessStatusCode();
        return ((await respuesta.Content.ReadFromJsonAsync<ClaveTemporal>(cancellationToken))?.Clave, null);
    }

    public async Task<(UsuarioCreado? Usuario, string? Error)> CrearUsuarioPlataformaAsync(
        string token, SolicitudCrearUsuarioPlataforma solicitud, CancellationToken cancellationToken)
    {
        using var peticion = CrearPeticion(HttpMethod.Post, "api/admin/plataforma/usuarios", token);
        peticion.Content = JsonContent.Create(solicitud);
        using var respuesta = await http.SendAsync(peticion, cancellationToken);
        if (respuesta.StatusCode == HttpStatusCode.Conflict)
            return (null, CodigosAdministracion.CorreoDuplicado);
        if (respuesta.StatusCode == HttpStatusCode.BadRequest)
            return (null, CodigosAdministracion.RolInvalido);
        respuesta.EnsureSuccessStatusCode();
        return (await respuesta.Content.ReadFromJsonAsync<UsuarioCreado>(cancellationToken), null);
    }

    public async Task<(UsuarioCreado? Usuario, string? Error)> CrearMiembroEmpresaAsync(
        string token, Guid empresaId, SolicitudCrearMiembroEmpresa solicitud, CancellationToken cancellationToken)
    {
        using var peticion = CrearPeticion(HttpMethod.Post, "api/admin/empresa/miembros", token, empresaId);
        peticion.Content = JsonContent.Create(solicitud);
        using var respuesta = await http.SendAsync(peticion, cancellationToken);
        if (respuesta.StatusCode == HttpStatusCode.Conflict)
            return (null, CodigosAdministracion.MembresiaDuplicada);
        respuesta.EnsureSuccessStatusCode();
        return (await respuesta.Content.ReadFromJsonAsync<UsuarioCreado>(cancellationToken), null);
    }

    public async Task<(string? ClaveTemporal, string? Error)> RestablecerClaveAsync(string token, string usuarioId, CancellationToken cancellationToken)
    {
        using var peticion = CrearPeticion(HttpMethod.Post, $"api/admin/usuarios/{Uri.EscapeDataString(usuarioId)}/restablecer-clave", token);
        using var respuesta = await http.SendAsync(peticion, cancellationToken);
        switch (respuesta.StatusCode)
        {
            case HttpStatusCode.NotFound: return (null, "no_encontrado");
            case HttpStatusCode.Forbidden: return (null, "permiso");
            case HttpStatusCode.BadRequest: return (null, await CodigoErrorAsync(respuesta, cancellationToken) ?? "datos");
        }
        respuesta.EnsureSuccessStatusCode();
        return ((await respuesta.Content.ReadFromJsonAsync<ClaveTemporal>(cancellationToken))?.Clave, null);
    }

    public async Task<CatalogosTransversales> CatalogosGlobalizacionAsync(string token, string? pais, CancellationToken cancellationToken)
    {
        var ruta = "api/globalizacion/catalogos";
        if (!string.IsNullOrWhiteSpace(pais)) ruta += $"?pais={Uri.EscapeDataString(pais)}";
        return await ConsultarAsync<CatalogosTransversales>(ruta, token, cancellationToken)
               ?? new CatalogosTransversales([], [], [], [], []);
    }

    public Task<ConfiguracionRegionalEmpresa?> ConfiguracionRegionalAsync(
        string token, Guid empresaId, CancellationToken cancellationToken) =>
        ConsultarAsync<ConfiguracionRegionalEmpresa>($"api/empresas/{empresaId:D}/configuracion-regional", token, cancellationToken);

    public async Task<(ConfiguracionRegionalEmpresa? Configuracion, string? Error)> GuardarConfiguracionRegionalAsync(
        string token, Guid empresaId, ConfiguracionRegionalEmpresa solicitud, CancellationToken cancellationToken)
    {
        using var peticion = CrearPeticion(HttpMethod.Put, $"api/empresas/{empresaId:D}/configuracion-regional", token);
        peticion.Content = JsonContent.Create(solicitud);
        using var respuesta = await http.SendAsync(peticion, cancellationToken);
        if (respuesta.StatusCode == HttpStatusCode.BadRequest)
            return (null, await CodigoErrorAsync(respuesta, cancellationToken) ?? "invalido");
        if (respuesta.StatusCode == HttpStatusCode.Forbidden) return (null, "permiso");
        respuesta.EnsureSuccessStatusCode();
        return (await respuesta.Content.ReadFromJsonAsync<ConfiguracionRegionalEmpresa>(cancellationToken), null);
    }

    public async Task<PaginaTasasCambio> TasasCambioAsync(
        string token, string monedaDestino, DateOnly? fecha, string? pais, CancellationToken cancellationToken)
    {
        var ruta = $"api/globalizacion/tasas?monedaDestino={Uri.EscapeDataString(monedaDestino)}";
        if (!string.IsNullOrWhiteSpace(pais)) ruta += $"&pais={Uri.EscapeDataString(pais)}";
        if (fecha is { } f) ruta += $"&fecha={f:yyyy-MM-dd}";
        return await ConsultarAsync<PaginaTasasCambio>(ruta, token, cancellationToken)
               ?? new PaginaTasasCambio([], monedaDestino, fecha ?? DateOnly.FromDateTime(DateTime.UtcNow));
    }

    public async Task<(ResultadoSincronizacionTasas? Resultado, string? Error)> SincronizarTasasAsync(
        string token, SolicitudSincronizarTasas solicitud, CancellationToken cancellationToken)
    {
        using var peticion = CrearPeticion(HttpMethod.Post, "api/globalizacion/tasas/sincronizar", token);
        peticion.Content = JsonContent.Create(solicitud);
        using var respuesta = await http.SendAsync(peticion, cancellationToken);
        if (respuesta.StatusCode == HttpStatusCode.BadRequest)
            return (null, await CodigoErrorAsync(respuesta, cancellationToken) ?? "proveedor_sin_datos");
        respuesta.EnsureSuccessStatusCode();
        return (await respuesta.Content.ReadFromJsonAsync<ResultadoSincronizacionTasas>(cancellationToken), null);
    }

    public async Task<string?> DesbloquearAsync(string token, string usuarioId, CancellationToken cancellationToken)
    {
        using var peticion = CrearPeticion(HttpMethod.Post, $"api/admin/usuarios/{Uri.EscapeDataString(usuarioId)}/desbloquear", token);
        using var respuesta = await http.SendAsync(peticion, cancellationToken);
        switch (respuesta.StatusCode)
        {
            case HttpStatusCode.NotFound: return "no_encontrado";
            case HttpStatusCode.Forbidden: return "permiso";
        }
        respuesta.EnsureSuccessStatusCode();
        return null;
    }

    public Task<DetalleUsuarioPlataforma?> UsuarioPlataformaAsync(string token, string usuarioId, CancellationToken cancellationToken) =>
        ConsultarAsync<DetalleUsuarioPlataforma>($"api/admin/usuarios/{Uri.EscapeDataString(usuarioId)}", token, cancellationToken);

    public async Task<string?> ActualizarUsuarioPlataformaAsync(
        string token, string usuarioId, SolicitudActualizarUsuarioPlataforma solicitud, CancellationToken cancellationToken)
    {
        using var peticion = CrearPeticion(HttpMethod.Put, $"api/admin/usuarios/{Uri.EscapeDataString(usuarioId)}", token);
        peticion.Content = JsonContent.Create(solicitud);
        using var respuesta = await http.SendAsync(peticion, cancellationToken);
        switch (respuesta.StatusCode)
        {
            case HttpStatusCode.NotFound: return "no_encontrado";
            case HttpStatusCode.Forbidden: return "permiso";
        }
        respuesta.EnsureSuccessStatusCode();
        return null;
    }

    public async Task<string?> CambiarEstadoUsuarioPlataformaAsync(
        string token, string usuarioId, bool activo, CancellationToken cancellationToken)
    {
        using var peticion = CrearPeticion(HttpMethod.Put, $"api/admin/usuarios/{Uri.EscapeDataString(usuarioId)}/estado", token);
        peticion.Content = JsonContent.Create(new SolicitudEstadoUsuarioPlataforma { Activo = activo });
        using var respuesta = await http.SendAsync(peticion, cancellationToken);
        switch (respuesta.StatusCode)
        {
            case HttpStatusCode.NotFound: return "no_encontrado";
            case HttpStatusCode.Forbidden: return "permiso";
            case HttpStatusCode.BadRequest: return await CodigoErrorAsync(respuesta, cancellationToken) ?? "datos";
        }
        respuesta.EnsureSuccessStatusCode();
        return null;
    }

    public async Task<string?> EliminarUsuarioPlataformaAsync(string token, string usuarioId, CancellationToken cancellationToken)
    {
        using var peticion = CrearPeticion(HttpMethod.Delete, $"api/admin/usuarios/{Uri.EscapeDataString(usuarioId)}", token);
        using var respuesta = await http.SendAsync(peticion, cancellationToken);
        switch (respuesta.StatusCode)
        {
            case HttpStatusCode.NotFound: return "no_encontrado";
            case HttpStatusCode.Forbidden: return "permiso";
            case HttpStatusCode.BadRequest: return await CodigoErrorAsync(respuesta, cancellationToken) ?? "datos";
        }
        respuesta.EnsureSuccessStatusCode();
        return null;
    }

    private static async Task<string?> CodigoErrorAsync(HttpResponseMessage respuesta, CancellationToken cancellationToken)
    {
        try
        {
            using var documento = await JsonDocument.ParseAsync(await respuesta.Content.ReadAsStreamAsync(cancellationToken), cancellationToken: cancellationToken);
            return documento.RootElement.ValueKind == JsonValueKind.Object
                && documento.RootElement.TryGetProperty("codigo", out var codigo) && codigo.ValueKind == JsonValueKind.String
                ? codigo.GetString()
                : null;
        }
        catch (JsonException)
        {
            return null;
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

    private static HttpRequestMessage CrearPeticion(HttpMethod metodo, string ruta, string token, Guid? empresaId = null)
    {
        var peticion = new HttpRequestMessage(metodo, ruta)
        {
            Headers = { Authorization = new AuthenticationHeaderValue("Bearer", token) }
        };
        if (empresaId is not null)
            peticion.Headers.TryAddWithoutValidation(CabecerasCliente.Empresa, empresaId.Value.ToString("D"));
        return peticion;
    }
}

/// <summary>Reenvia al API la IP y el agente del navegador; el API los registra en sesiones y auditoria.</summary>
public sealed class CabecerasClienteHandler(IHttpContextAccessor accesor) : DelegatingHandler
{
    protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        if (accesor.HttpContext is { } context)
        {
            if (context.Connection.RemoteIpAddress is { } ip) request.Headers.TryAddWithoutValidation(CabecerasCliente.Ip, ip.ToString());
            var agente = context.Request.Headers.UserAgent.ToString();
            if (agente.Length > 0) request.Headers.TryAddWithoutValidation(CabecerasCliente.Agente, agente.Length <= 256 ? agente : agente[..256]);
            if (!request.Headers.Contains(CabecerasCliente.Empresa)
                && Guid.TryParse(context.User.FindFirstValue(EndpointsSesion.EmpresaClaim), out var empresaId))
            {
                request.Headers.TryAddWithoutValidation(CabecerasCliente.Empresa, empresaId.ToString("D"));
            }
        }
        return base.SendAsync(request, cancellationToken);
    }
}
