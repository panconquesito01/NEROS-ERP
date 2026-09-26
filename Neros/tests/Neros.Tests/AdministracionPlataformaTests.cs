using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Security.Claims;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Neros.Application.Seguridad;
using Neros.Contracts.Administracion;
using Neros.Contracts.Autenticacion;
using Neros.Persistence;
using Neros.Persistence.Seguridad;
using Xunit;

namespace Neros.Tests;

public sealed class AdministracionPlataformaTests(EntornoPruebas entorno) : IClassFixture<EntornoPruebas>
{
    [Fact]
    public async Task AdminGlobal_CreaEmpresaYUsuarioConMembresiaAsync()
    {
        var admin = await CrearAdministradorGlobalAsync();
        using var cliente = Cliente(await entorno.EntrarAsync(admin));

        var codigo = $"T{Guid.NewGuid():N}"[..8].ToUpperInvariant();
        using var crearEmpresa = await cliente.PostAsJsonAsync("api/admin/plataforma/empresas",
            new SolicitudCrearEmpresa { Codigo = codigo, Nombre = "Empresa SaaS Test", Identificacion = $"NIT-{codigo}" });
        Assert.Equal(HttpStatusCode.OK, crearEmpresa.StatusCode);
        var empresa = (await crearEmpresa.Content.ReadFromJsonAsync<EmpresaAdministrada>())!;

        var correo = $"saas_{Guid.NewGuid():N}@test.local";
        using var crearUsuario = await cliente.PostAsJsonAsync("api/admin/plataforma/usuarios",
            new SolicitudCrearUsuarioPlataforma
            {
                Correo = correo,
                Perfil = new PerfilPersonaUsuario
                {
                    PrimerNombre = "Usuario",
                    PrimerApellido = "SaaS",
                    TipoDocumento = "CC",
                    NumeroDocumento = "9876543210"
                },
                MembresiaInicial = new SolicitudAsignarEmpresa { EmpresaId = empresa.Id, Rol = "Consulta" }
            });
        Assert.Equal(HttpStatusCode.OK, crearUsuario.StatusCode);
        var creado = await crearUsuario.Content.ReadFromJsonAsync<UsuarioCreado>();
        Assert.NotEmpty(creado!.ClaveTemporal);

        await using var scope = entorno.Api.Services.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<NerosDbContext>();
        Assert.True(await db.UsuariosEmpresas.AnyAsync(m => m.EmpresaId == empresa.Id && m.Rol == "Consulta"));
    }

    private async Task<CuentaPrueba> CrearAdministradorGlobalAsync()
    {
        var cuenta = await entorno.CrearCuentaAsync();
        await using var scope = entorno.Api.Services.CreateAsyncScope();
        var usuarios = scope.ServiceProvider.GetRequiredService<UserManager<Usuario>>();
        var usuario = (await usuarios.FindByIdAsync(cuenta.Id))!;
        Assert.True((await usuarios.AddClaimAsync(usuario, new Claim(ClaimTypes.Role, Permisos.RolAdministradorGlobal))).Succeeded);
        return cuenta;
    }

    private HttpClient Cliente(AccesoConcedido acceso)
    {
        var cliente = new HttpClient(new HttpClientHandler { AllowAutoRedirect = false }) { BaseAddress = entorno.Http.BaseAddress };
        cliente.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", acceso.Token);
        return cliente;
    }
}
