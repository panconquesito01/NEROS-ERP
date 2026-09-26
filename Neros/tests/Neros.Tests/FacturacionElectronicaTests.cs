using Neros.Domain.Facturacion;
using Neros.Domain.Facturacion.Dian;
using Xunit;

namespace Neros.Tests;

public sealed class FacturacionElectronicaTests
{
    private static RangoNumeracionFiscal RangoActivo() => new(
        Guid.NewGuid(), TipoDocumentoFiscal.FacturaVenta, "FV", 1, 1000, 10,
        new DateOnly(2026, 1, 1), new DateOnly(2026, 12, 31), "18764000000001", EstadoNumeracionFiscal.Activo);

    private static DatosEmisorSnapshot Emisor() => new(
        "900123456", "Emisor Prueba S.A.S.", "O-13", "Resolucion227-2025", "Impuestos-v1", "Neros-1.0.0");

    private static DatosAdquirenteSnapshot Adquirente() => new("31", "800987654", "Cliente Demo", "Calle 1");

    [Fact]
    public void Numeracion_ReservaSinHuecos_YAlertaAgotamiento()
    {
        var reserva = MotorNumeracionFiscal.ReservarSiguiente(RangoActivo(), new DateOnly(2026, 6, 1));
        Assert.Equal(11, reserva.NumeroAsignado);
        Assert.Equal("FV11", reserva.NumeroPresentacion);
        Assert.True(MotorNumeracionFiscal.DebeAlertarAgotamiento(
            RangoActivo() with { Actual = 995, Hasta = 1000 }));
    }

    [Fact]
    public void Emision_RequiereSnapshotImpuestosYAdquirente()
    {
        var rango = RangoActivo();
        Assert.Throws<InvalidOperationException>(() => MotorEmisionComercial.Emitir(
            EstadoDocumentoComercial.Borrador, rango, new DateOnly(2026, 6, 1), DateTimeOffset.UtcNow,
            "COP", 1m, Emisor(), Adquirente(), null, null, 100m, 19m, 0m, 119m));
    }

    [Fact]
    public void Emision_CreaSnapshotLegal()
    {
        var (snapshot, estadoElectronico) = MotorEmisionComercial.Emitir(
            EstadoDocumentoComercial.Borrador, RangoActivo(), new DateOnly(2026, 6, 1),
            new DateTimeOffset(2026, 6, 1, 15, 0, 0, TimeSpan.Zero),
            "COP", 1m, Emisor(), Adquirente(), Guid.NewGuid(), 3, 100m, 19m, 0m, 119m);
        Assert.Equal(EstadoDocumentoElectronico.Borrador, estadoElectronico);
        Assert.Equal("FV11", snapshot.NumeroPresentacion);
        Assert.Equal("900123456", snapshot.Emisor.Nit);
        Assert.Equal(119m, snapshot.Total);
    }

    [Fact]
    public void MaquinaEstadosElectronico_RespetaFlujoPlan50()
    {
        var estado = MaquinaEstadosDocumentoElectronico.GenerarXml(EstadoDocumentoElectronico.Borrador);
        estado = MaquinaEstadosDocumentoElectronico.Firmar(estado);
        estado = MaquinaEstadosDocumentoElectronico.Enviar(estado);
        estado = MaquinaEstadosDocumentoElectronico.MarcarValidado(estado);
        Assert.Equal(EstadoDocumentoElectronico.Validado, estado);
        Assert.True(MaquinaEstadosDocumentoElectronico.PdfIndicaExitoDian(estado));
        Assert.False(MaquinaEstadosDocumentoElectronico.PdfIndicaExitoDian(EstadoDocumentoElectronico.Generado));
    }

    [Fact]
    public void Idempotencia_MismaClaveMismoHashDevuelveRespuesta()
    {
        var resultado = MotorIdempotenciaEmision.Evaluar("k1", "k1", "hash", "hash", "{\"ok\":true}");
        Assert.True(resultado.EsRepeticion);
        Assert.Equal("{\"ok\":true}", resultado.RespuestaSerializada);
    }

    [Fact]
    public async Task FlujoElectronico_EnvioSandboxValidaConCufe()
    {
        var envio = new EnvioDianSandbox();
        var (generado, hash) = MotorFlujoElectronico.Generar(EstadoDocumentoElectronico.Borrador, "<Invoice/>");
        var firmado = MotorFlujoElectronico.Firmar(generado);
        var (validado, respuesta) = await MotorFlujoElectronico.EnviarAsync(
            firmado, AmbienteDian.Habilitacion, hash, "idem-1", envio);
        Assert.Equal(EstadoDocumentoElectronico.Validado, validado);
        Assert.True(respuesta.Exito);
        Assert.False(string.IsNullOrWhiteSpace(respuesta.Cufe));
    }

    private sealed class EnvioDianSandbox : IEnvioDocumentoElectronicoDian
    {
        public Task<ResultadoTransmisionDian> EnviarAsync(
            AmbienteDian ambiente, string hashXml, string idempotencyKey, CancellationToken cancellationToken = default)
        {
            if (ambiente != AmbienteDian.Habilitacion)
                return Task.FromResult(new ResultadoTransmisionDian(false, null, null, "AMB", "Solo sandbox en pruebas."));
            return Task.FromResult(new ResultadoTransmisionDian(
                true, "TRACK-1", $"CUFE-{hashXml[..8]}", "00", "Documento validado (habilitacion)."));
        }
    }
}
