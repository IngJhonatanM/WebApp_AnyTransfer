using EDIBANK.Models.Monitor;
using EDIBANK.Models.Users_EdiWeb;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EDIBANK.Conf_Db_With_Entity;

/// <summary>
/// _appDbContext extends Entity Framework's IdentityDBContext,
/// representing a database session and used to customize Identity's schema to suit application needs.
/// </summary>
public class AppDbContext(DbContextOptions options) : IdentityDbContext<AppUser>(options)
{
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.UseCollation("Latin1_General_100_CI_AI_SC_UTF8");
        modelBuilder.Entity<Intercambio>()
                    .ToTable(static void (TableBuilder<Intercambio> builder) =>
                    {
                        builder.HasCheckConstraint("CK_tip_intercambio", "[tip_intercambio] IN (0, 1, 2)");
                        builder.HasCheckConstraint("CK_status", "[status] IN (0, 1, 2, 3)");
                    })
                    .Property(static bool (Intercambio i) => i.EsRecarga)
                    .HasDefaultValue(false);
        modelBuilder.Entity<HistoricoIntercambio>()
                    .ToTable(static void (TableBuilder<HistoricoIntercambio> builder) =>
                    {
                        builder.HasCheckConstraint("CK_tip_intercambio", "[tip_intercambio] IN (0, 1, 2)");
                        builder.HasCheckConstraint("CK_status", "[status] IN (0, 1, 2, 3)");
                    })
                    .Property(static bool (HistoricoIntercambio h) => h.EsRecarga)
                    .HasDefaultValue(false);
    }

    public required DbSet<Intercambio> Intercambios { get; init; }
    public required DbSet<HistoricoIntercambio> HistoricoIntercambios { get; init; }
    public required DbSet<EDI> EDIs { get; init; }
    public required DbSet<Auditoria> Auditorias { get; init; }
    public required DbSet<Parametro> Parametros { get; init; }
}
