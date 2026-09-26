using Neros.Integration.Worker;
using Neros.Messaging.Abstractions;
using Neros.Messaging.RabbitMQ;
using Neros.Messaging.Sql;

var builder = Host.CreateApplicationBuilder(args);
builder.Services.Configure<OpcionesRabbitMq>(builder.Configuration.GetSection("RabbitMQ"));
builder.Services.AddSingleton<BusEventosRabbitMq>();
builder.Services.AddSingleton<IEventBus>(sp => sp.GetRequiredService<BusEventosRabbitMq>());

builder.Services.AddSingleton<DespachadorOutboxModulos>(sp =>
{
    var configuracion = sp.GetRequiredService<IConfiguration>();
    var bus = sp.GetRequiredService<IEventBus>();
    var modulos = new[] { "Ventas", "Compras", "Nomina", "Facturacion" };
    var despachadores = new List<DespachadorOutbox>();
    foreach (var modulo in modulos)
    {
        var cadena = configuracion.GetConnectionString(modulo);
        if (string.IsNullOrWhiteSpace(cadena))
            continue;
        despachadores.Add(new DespachadorOutbox(new AlmacenOutboxSql(cadena), bus));
    }
    if (despachadores.Count == 0)
    {
        var ventas = configuracion.GetConnectionString("Ventas")
            ?? throw new InvalidOperationException("Configurar ConnectionStrings para modulos con outbox (Ventas, Compras, Nomina, Facturacion).");
        despachadores.Add(new DespachadorOutbox(new AlmacenOutboxSql(ventas), bus));
    }
    return new DespachadorOutboxModulos(despachadores);
});

builder.Services.AddHostedService<ServicioDespachoOutbox>();
builder.Services.AddHostedService<ServicioConsumoIntegracion>();

await builder.Build().RunAsync();
