using Neros.Domain.Globalizacion;
using Neros.Domain.Nomina;
using Neros.Domain.Nomina.Electronica;
using Neros.Domain.Nomina.Legal;
using Xunit;

namespace Neros.Tests;

public sealed class NominaLegalColombiaTests
{
    private static PoliticaRedondeo PoliticaCop() => CatalogoIso.PoliticaPorDefecto("COP");

    private static ParametrosLegalesColombia PaqueteAprobado() => new(
        Guid.NewGuid(), MotorPaqueteLegalColombia.CodigoPaquete, "2026-01-TEST",
        new DateOnly(2026, 1, 1), new DateOnly(2026, 12, 31),
        1_423_500m, 4m, 4m, 8.5m, 12m, 0.522m, 4m,
        950_000m, 0m, EstadoPaqueteLegalNomina.Aprobado);

    private static ContratoNomina Contrato() => new(
        Guid.NewGuid(), Guid.NewGuid(), new DateOnly(2026, 1, 1), null, "Indefinido",
        3_000_000m, 48m, EstadoContrato.Activo);

    private static PeriodoNominaRango Periodo() => new(
        Guid.NewGuid(), new DateOnly(2026, 3, 1), new DateOnly(2026, 3, 31), EstadoPeriodoNomina.Abierto);

    [Fact]
    public void Paquete_NoAprobado_RechazaCalculo()
    {
        var pendiente = PaqueteAprobado() with { EstadoAprobacion = EstadoPaqueteLegalNomina.PendienteEspecialista };
        Assert.Throws<InvalidOperationException>(() =>
            MotorPaqueteLegalColombia.ValidarParaCalculo(pendiente, new DateOnly(2026, 3, 31)));
    }

    [Fact]
    public void Liquidacion_ConPaqueteAprobado_IncluyeSeguridadSocial()
    {
        var salario = new ConceptoNominaDefinicion(
            Guid.NewGuid(), "SAL", "Salario", NaturalezaConceptoNomina.Devengo,
            TipoFormulaConceptoNomina.Fijo, 3_000_000m, null, true);
        var resultado = OrquestadorLiquidacionNomina.Procesar(
            Contrato(), Periodo(), [salario], new Dictionary<Guid, decimal?>(),
            30m, 192m, PoliticaCop(), "Neros-1.0.0", MotorPaqueteLegalColombia.CodigoPaquete, PaqueteAprobado());
        Assert.Null(resultado.AdvertenciaLegal);
        Assert.True(resultado.Snapshot.AplicaReglasLegalesColombia);
        Assert.NotNull(resultado.DetalleLegalColombia);
        Assert.Equal(120_000m, resultado.DetalleLegalColombia!.SeguridadSocial.AporteSaludEmpleado);
        Assert.Equal(120_000m, resultado.DetalleLegalColombia.SeguridadSocial.AportePensionEmpleado);
        Assert.Equal(2_760_000m, resultado.Totales.NetoPagar);
    }

    [Fact]
    public async Task NominaElectronica_FlujoSandboxConCune()
    {
        var envio = new EnvioNominaSandbox();
        MotorFlujoNominaElectronica.ValidarPrecondicion(EstadoLiquidacionNominaContabilizable.Contabilizada);
        var (generado, hash) = MotorFlujoNominaElectronica.Generar(EstadoNominaElectronica.Borrador, "<NominaIndividual/>");
        var firmado = MotorFlujoNominaElectronica.Firmar(generado);
        var (validado, respuesta) = await MotorFlujoNominaElectronica.EnviarAsync(
            firmado, AmbienteDianNomina.Habilitacion, hash, "nom-1", envio);
        Assert.Equal(EstadoNominaElectronica.Validado, validado);
        Assert.False(string.IsNullOrWhiteSpace(respuesta.Cune));
    }

    private sealed class EnvioNominaSandbox : IEnvioNominaElectronicaDian
    {
        public Task<ResultadoTransmisionNominaElectronica> EnviarAsync(
            AmbienteDianNomina ambiente, string hashXml, string idempotencyKey, CancellationToken cancellationToken = default)
        {
            if (ambiente != AmbienteDianNomina.Habilitacion)
                return Task.FromResult(new ResultadoTransmisionNominaElectronica(false, null, null, "AMB", "Solo habilitacion."));
            return Task.FromResult(new ResultadoTransmisionNominaElectronica(
                true, "NE-TRACK-1", $"CUNE-{hashXml[..8]}", "00", "Nomina validada (habilitacion)."));
        }
    }
}
