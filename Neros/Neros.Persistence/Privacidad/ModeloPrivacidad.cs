namespace Neros.Persistence.Privacidad;

public sealed class CatalogoDatoPersonal
{
    public Guid Id { get; set; }
    public string Campo { get; set; } = string.Empty;
    public string Modulo { get; set; } = string.Empty;
    public string Clasificacion { get; set; } = string.Empty;
    public string Finalidad { get; set; } = string.Empty;
    public string Origen { get; set; } = string.Empty;
    public string RetencionCodigo { get; set; } = string.Empty;
    public string? PermisoRequerido { get; set; }
    public bool Activo { get; set; }
}

public sealed class DocumentoLegal
{
    public Guid Id { get; set; }
    public string Codigo { get; set; } = string.Empty;
    public string Nombre { get; set; } = string.Empty;
    public bool RequiereAceptacion { get; set; }
    public int Orden { get; set; }
    public bool Activo { get; set; }
    public ICollection<DocumentoLegalVersion> Versiones { get; set; } = [];
}

public sealed class DocumentoLegalVersion
{
    public Guid Id { get; set; }
    public Guid DocumentoId { get; set; }
    public DocumentoLegal Documento { get; set; } = null!;
    public int Version { get; set; }
    public string Contenido { get; set; } = string.Empty;
    public string HashContenido { get; set; } = string.Empty;
    public DateTime VigenteDesde { get; set; }
    public DateTime? VigenteHasta { get; set; }
    public string? PublicadoPor { get; set; }
}

public sealed class AceptacionLegal
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid DocumentoId { get; set; }
    public Guid VersionId { get; set; }
    public string UsuarioId { get; set; } = string.Empty;
    public DateTime FechaUtc { get; set; }
    public string? Ip { get; set; }
    public string? AgenteUsuario { get; set; }
    public string HashContenido { get; set; } = string.Empty;
    public string FormaAceptacion { get; set; } = string.Empty;
}

public sealed class DefinicionCookie
{
    public Guid Id { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string Almacenamiento { get; set; } = string.Empty;
    public string Proveedor { get; set; } = string.Empty;
    public string Categoria { get; set; } = string.Empty;
    public string Finalidad { get; set; } = string.Empty;
    public string Duracion { get; set; } = string.Empty;
    public bool PrimeraParte { get; set; }
    public string? Dominio { get; set; }
    public bool Esencial { get; set; }
    public string? UrlPolitica { get; set; }
    public int Orden { get; set; }
    public bool Activo { get; set; }
}

public sealed class RetencionPolicy
{
    public string Codigo { get; set; } = string.Empty;
    public string TipoInformacion { get; set; } = string.Empty;
    public string Jurisdiccion { get; set; } = string.Empty;
    public int? PeriodoDias { get; set; }
    public string? PeriodoDescripcion { get; set; }
    public string InicioComputo { get; set; } = string.Empty;
    public string Fundamento { get; set; } = string.Empty;
    public string AccionFinal { get; set; } = string.Empty;
    public bool Activo { get; set; }
}

public sealed class LegalHold
{
    public Guid Id { get; set; }
    public string Referencia { get; set; } = string.Empty;
    public string Alcance { get; set; } = string.Empty;
    public string Motivo { get; set; } = string.Empty;
    public DateTime FechaInicioUtc { get; set; }
    public DateTime? FechaFinUtc { get; set; }
    public bool Activo { get; set; }
}
