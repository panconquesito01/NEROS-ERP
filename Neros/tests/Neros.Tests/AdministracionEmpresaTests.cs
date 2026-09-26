using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using Neros.Contracts.Administracion;
using Neros.Contracts.Autenticacion;
using Neros.Persistence.Seguridad;
using Xunit;

namespace Neros.Tests;

public sealed class AdministracionEmpresaTests(EntornoPruebas entorno) : IClassFixture<EntornoPruebas>
{
    [Fact]
    public async Task AdminEmpresa_CreaMiembroYListaAsync()
    {
        var admin = await entorno.CrearCuentaAsync();
        using var cliente = Cliente(await entorno.EntrarAsync(admin), entorno.EmpresaPrimera);

        var correo = $"miembro_{Guid.NewGuid():N}@test.local";
        using var crear = await cliente.PostAsJsonAsync("api/admin/empresa/miembros",
            new SolicitudCrearMiembroEmpresa
            {
                Correo = correo,
                Rol = "Operador",
                Perfil = new PerfilPersonaUsuario
                {
                    PrimerNombre = "Miembro",
                    PrimerApellido = "Demo",
                    TipoDocumento = "CC",
                    NumeroDocumento = "1234567890"
                }
            });
        Assert.Equal(HttpStatusCode.OK, crear.StatusCode);
        var creado = await crear.Content.ReadFromJsonAsync<UsuarioCreado>();
        Assert.NotNull(creado);
        Assert.NotEmpty(creado!.ClaveTemporal);

        var pagina = await cliente.GetFromJsonAsync<PaginaMiembrosEmpresa>("api/admin/empresa/miembros");
        Assert.Contains(pagina!.Miembros, m => m.Correo == correo);
    }

    private HttpClient Cliente(AccesoConcedido acceso, Guid empresaId)
    {
        var cliente = new HttpClient(new HttpClientHandler { AllowAutoRedirect = false }) { BaseAddress = entorno.Http.BaseAddress };
        cliente.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", acceso.Token);
        cliente.DefaultRequestHeaders.Add(CabecerasCliente.Empresa, empresaId.ToString("D"));
        return cliente;
    }
}
