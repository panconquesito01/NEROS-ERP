using System.Collections.Concurrent;

namespace Neros.Domain.Inventario;

/// <summary>Libro en memoria con bloqueo por producto/bodega (plan §49 concurrencia).</summary>
public sealed class LibroInventario(PoliticaBodega politica)
{
    private readonly ConcurrentDictionary<ClaveExistencia, object> _bloqueos = new();
    private readonly ConcurrentDictionary<ClaveExistencia, List<MovimientoInventarioPendiente>> _movimientos = new();
    private readonly PoliticaBodega _politica = politica;

    public IReadOnlyList<MovimientoInventarioPendiente> Movimientos(ClaveExistencia clave) =>
        _movimientos.TryGetValue(clave, out var lista) ? lista : [];

    public void Registrar(MovimientoInventarioPendiente movimiento, ClaveExistencia clave)
    {
        var candado = _bloqueos.GetOrAdd(clave, _ => new object());
        lock (candado)
        {
            var lista = _movimientos.GetOrAdd(clave, _ => []);
            var candidata = lista.Append(movimiento).ToList();
            RecosteadorInventario.Recostear(candidata, _politica);
            lista.Add(movimiento);
        }
    }

    public async Task RegistrarSalidaConcurrenteAsync(MovimientoInventarioPendiente movimiento, ClaveExistencia clave)
    {
        await Task.Run(() => Registrar(movimiento, clave));
    }

    public EstadoExistencia EstadoActual(ClaveExistencia clave)
    {
        var candado = _bloqueos.GetOrAdd(clave, _ => new object());
        lock (candado)
        {
            var (estado, _) = RecosteadorInventario.Recostear(Movimientos(clave), _politica);
            return estado;
        }
    }
}
