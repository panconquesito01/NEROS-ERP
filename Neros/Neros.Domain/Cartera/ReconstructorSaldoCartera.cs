namespace Neros.Domain.Cartera;

/// <summary>Reconstruye saldos desde movimientos; la proyeccion debe coincidir (plan §43, §63).</summary>
public static class ReconstructorSaldoCartera
{
    public static IReadOnlyList<SaldoDocumentoCartera> ReconstruirPorDocumento(
        IEnumerable<Guid> documentos, IReadOnlyList<MovimientoCarteraEntrada> movimientos)
        => documentos.Select(id => CalculadorSaldoCartera.SaldoDocumento(id, movimientos)).ToList();

    public static void ValidarProyeccion(Guid documentoId, decimal saldoProyectado, IReadOnlyList<MovimientoCarteraEntrada> movimientos)
    {
        var reconstruido = CalculadorSaldoCartera.SaldoDocumento(documentoId, movimientos);
        if (reconstruido.Saldo != saldoProyectado)
            throw new InvalidOperationException(
                $"Proyeccion de saldo inconsistente en documento {documentoId}: {saldoProyectado} != {reconstruido.Saldo}.");
    }
}
