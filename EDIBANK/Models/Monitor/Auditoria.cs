using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EDIBANK.Models.Monitor;

[Table("auditoria")]
public class Auditoria
{
    [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity), Column("id_auditoria", TypeName = "bigint", Order = 0)]
    public long Id { get; init; }

    [Column("fec_auditoria", TypeName = "datetime2", Order = 1)]
    public required DateTime Fecha { get; init; }

    [Column("login_usuario", TypeName = "varchar(192)", Order = 2)]
    public required string Usuario { get; init; }

    [Column("ip_remota", TypeName = "varchar(39)", Order = 3), Unicode(false)]
    public required string IPRemota { get; init; }

    [Column("modulo", TypeName = "varchar(192)", Order = 4)]
    public required string Modulo { get; init; }

    [Column("operacion", TypeName = "varchar(48)", Order = 5)]
    public required string Operacion { get; init; }

    [Column("comentarios", TypeName = "varchar(768)", Order = 6)]
    public required string? Comentario { get; init; }
}
