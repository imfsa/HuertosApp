using SQLite;

[Table("RegistroCosecha")]
public class RegistroCosecha
{
    [PrimaryKey, AutoIncrement]
    public int Id { get; set; }

    public long TreeId { get; set; }
    public string FechaCosecha { get; set; } = "";   // yyyy-MM-dd

    public int? Temporada { get; set; }
    public string Genotipo { get; set; } = "";
    public string Especie { get; set; } = "";
    public int? Replica { get; set; }
    public int? Fila { get; set; }
    public int? Columna { get; set; }
    public string Predio { get; set; } = "";
    public int? CodHuerto { get; set; }
    public string HuertoNombre { get; set; } = "";

    public decimal Kilos { get; set; }
    public string Cosechador { get; set; } = "";

    public bool Despachado { get; set; } = false;
    public long? DespachoId { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.Now;

    public int? UsuarioId { get; set; }   // <- NUEVO
    public bool Sincronizado { get; set; } = false;
}
