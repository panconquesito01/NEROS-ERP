using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Neros.Persistence;
using Neros.Persistence.Seguridad;

namespace Neros.Api.Seguridad;

public static class ComandosAdministracion
{
    public static async Task EjecutarAsync(IServiceProvider services, string[] argumentos)
    {
        await using var scope = services.CreateAsyncScope();
        var database = scope.ServiceProvider.GetRequiredService<NerosDbContext>();
        var inicializar = argumentos.Contains("--inicializar-admin");
        var administradorGlobal = argumentos.Contains("--administrador-global");
        if (administradorGlobal && !inicializar)
        {
            throw new InvalidOperationException("El permiso global solo se concede durante el alta inicial explicita.");
        }
        if (inicializar && await database.Users.AnyAsync())
        {
            throw new InvalidOperationException("Ya existen usuarios. El alta inicial no se puede repetir.");
        }
        var correo = Leer("Correo del usuario", 256);
        if (!new EmailAddressAttribute().IsValid(correo))
        {
            throw new InvalidOperationException("Correo no valido.");
        }
        var usuarios = scope.ServiceProvider.GetRequiredService<UserManager<Usuario>>();
        var usuario = await usuarios.FindByEmailAsync(correo);
        if (!inicializar && usuario is null)
        {
            throw new InvalidOperationException("El usuario no existe.");
        }
        await using var transaccion = await database.Database.BeginTransactionAsync(System.Data.IsolationLevel.Serializable);
        if (inicializar)
        {
            if (await database.Users.AnyAsync())
            {
                throw new InvalidOperationException("El alta inicial ya fue completada.");
            }
            usuario = new Usuario { UserName = correo, Email = correo, Nombre = Leer("Nombre", 160), EmailConfirmed = true };
            Console.Write("Clave (minimo 12 caracteres, mayuscula, minuscula, numero y simbolo): ");
            var clave = LeerClave();
            Console.Write("Confirmar clave: ");
            if (clave != LeerClave())
            {
                throw new InvalidOperationException("Las claves no coinciden.");
            }
            var resultado = await usuarios.CreateAsync(usuario, clave);
            if (!resultado.Succeeded)
            {
                throw new InvalidOperationException(string.Join("; ", resultado.Errors.Select(error => error.Code)));
            }
            if (administradorGlobal)
            {
                var permiso = await usuarios.AddClaimAsync(usuario, new Claim(ClaimTypes.Role, "AdministradorGlobal"));
                if (!permiso.Succeeded)
                {
                    throw new InvalidOperationException(string.Join("; ", permiso.Errors.Select(error => error.Code)));
                }
                database.EventosAcceso.Add(new EventoAcceso
                {
                    UsuarioId = usuario.Id, Fecha = DateTimeOffset.UtcNow, Accion = "Alta local de administrador global"
                });
            }
        }
        var codigo = Leer("Codigo de empresa", 20).ToUpperInvariant();
        var empresa = await database.Empresas.SingleOrDefaultAsync(entidad => entidad.Codigo == codigo);
        if (empresa is null)
        {
            empresa = new Empresa { Codigo = codigo, Nombre = Leer("Razon social", 160), Identificacion = Leer("Identificacion fiscal", 30) };
            database.Empresas.Add(empresa);
        }
        var membresia = await database.UsuariosEmpresas.FindAsync(usuario!.Id, empresa.Id);
        if (membresia is not null)
        {
            throw new InvalidOperationException("La membresia ya existe. No se modificaron permisos.");
        }
        var rol = inicializar ? "Administrador" : Leer("Rol (Administrador, Operador o Consulta)", 30);
        if (rol is not ("Administrador" or "Operador" or "Consulta"))
        {
            throw new InvalidOperationException("Rol no valido.");
        }
        database.UsuariosEmpresas.Add(new UsuarioEmpresa { UsuarioId = usuario.Id, Empresa = empresa, Rol = rol });
        database.EventosAcceso.Add(new EventoAcceso
        {
            UsuarioId = usuario.Id, EmpresaId = empresa.Id, Fecha = DateTimeOffset.UtcNow, Accion = "Alta local de membresia"
        });
        await database.SaveChangesAsync();
        await transaccion.CommitAsync();
        Console.WriteLine("Alta completada. Ya puedes acceder a Neros ERP.");
    }

    private static string Leer(string etiqueta, int maximo)
    {
        Console.Write($"{etiqueta}: ");
        var valor = Console.ReadLine()?.Trim();
        if (string.IsNullOrWhiteSpace(valor) || valor.Length > maximo)
        {
            throw new InvalidOperationException($"{etiqueta}: longitud no valida.");
        }
        return valor;
    }

    private static string LeerClave()
    {
        var valor = new StringBuilder();
        while (true)
        {
            var tecla = Console.ReadKey(intercept: true);
            if (tecla.Key == ConsoleKey.Enter)
            {
                Console.WriteLine();
                return valor.ToString();
            }
            if (tecla.Key == ConsoleKey.Backspace && valor.Length > 0)
            {
                valor.Length--;
            }
            else if (!char.IsControl(tecla.KeyChar) && valor.Length < 128)
            {
                valor.Append(tecla.KeyChar);
            }
        }
    }
}