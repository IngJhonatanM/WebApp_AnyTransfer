using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EDIBANK.Models.Monitor;

[Table("parametros")]
public class Parametro
{
    [Key, DatabaseGenerated(DatabaseGeneratedOption.None), Column("email_svc", TypeName = "varchar(192)", Order = 0)]
    public required string ServiceEmail { get; init; }

    [Column("des_svc", TypeName = "varchar(192)", Order = 1)]
    public required string ServiceDisplayName { get; init; }

    [Column("host_smtp", TypeName = "varchar(48)", Order = 2)]
    public required string SmtpHost { get; init; }

    [Column("port_smtp", TypeName = "int", Order = 3)]
    public required int SmtpPort { get; init; }

    [Column("pass_smtp", TypeName = "varchar(48)", Order = 4)]
    public required string SmtpPassword { get; init; }
}
