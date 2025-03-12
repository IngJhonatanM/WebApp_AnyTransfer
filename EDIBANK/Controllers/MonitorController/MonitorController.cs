using EDIBANK.Conf_Db_With_Entity;
using EDIBANK.Models.Monitor;
using EDIBANK.Models.Users_EdiWeb;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Data;

namespace EDIBANK.Controllers.MonitorController;

public class MonitorController(AppDbContext context, UserManager<AppUser> userManager) : Controller
{
    private readonly AppDbContext _context = context;
    private readonly UserManager<AppUser> _userManager = userManager;

    // GET: Monitor/Intercambios
    [HttpGet]
    [Authorize(Roles = "User,Admin")]
    public async Task<IActionResult> Intercambios(bool? mostrarEntradas, string? ediActual, DateTime? desde, DateTime? hasta, int? talla, int? pagina, string? mensaje)
    {
        mostrarEntradas ??= true;
        ediActual ??= User.IsInRole("User") switch
        {
            true => (await _userManager.GetUserAsync(User))!.EDIId,
            false => (await _context.EDIs.DefaultAsync()).Id
        };

        IQueryable<Intercambio> intercambiosNT = _context.Intercambios.AsNoTracking();
        DateTime hoy = DateTime.Today;
        DateTime ayer = hoy.AddDays(-1.0);
        DateTime menor = await intercambiosNT.AnyAsync(bool (Intercambio i) => i.Cargado < ayer) switch
        {
            true => (await intercambiosNT.MinAsync(DateTime (Intercambio i) => i.Cargado)).Date,
            false => ayer
        };

        desde ??= ayer;
        hasta ??= hoy;
        if (hasta < desde)
        {
            ModelState.AddModelError(string.Empty, "Desde no puede ser mayor que Hasta.");
        }
        hasta = hasta.Value.AddDays(1.0);
        talla = talla switch
        {
            null => 15,
            < 5 => 5,
            > 50 => 50,
            _ => talla.Value
        };
        pagina ??= 0;

        IOrderedQueryable<Intercambio> intercambios = mostrarEntradas switch
        {
            true => from i in intercambiosNT
                    where desde <= i.Cargado && i.Cargado < hasta && i.ReceptorId == ediActual
                    orderby i.Cargado descending
                    select i,
            false => from i in intercambiosNT
                     where desde <= i.Cargado && i.Cargado < hasta && i.EmisorId == ediActual && !i.EsRecarga
                     orderby i.Cargado descending
                     select i
        };

        if (mensaje is not null)
        {
            ModelState.AddModelError(string.Empty, mensaje);
        }
        return View(new MonitorViewModel
        {
            MostrarEntradas = mostrarEntradas.Value,
            EDIActual = ediActual,
            Menor = $"{menor:yyyy-MM-dd}",
            Mayor = $"{hoy:yyyy-MM-dd}",
            Desde = desde.Value,
            Hasta = hasta.Value.AddDays(-1.0),
            Talla = talla.Value,
            Pagina = pagina.Value,
            TotalPaginas = Math.DivRem(await intercambios.CountAsync(), talla.Value) switch
            {
                (int n, 0) => n,
                (int n, _) => n + 1
            },
            Intercambios = await intercambios.Skip(pagina.Value * talla.Value)
                                             .Take(talla.Value)
                                             .ToListAsync()
        });
    }

    // POST: Monitor/Recargar
    [HttpPost, ValidateAntiForgeryToken]
    [Authorize(Roles = "User,Admin")]
    public async Task<IActionResult> Recargar(bool mostrarEntradas, string ediActual, DateTime desde, DateTime hasta, int talla, int pagina, string id)
    {
        Helper<MonitorController> helper = new(_context, (await _userManager.GetUserAsync(User))!, HttpContext);
        string? mensaje = null;

        try
        {
            if (await _context.Intercambios.FindAsync(id) is Intercambio origen)
            {
                DateTime ahora = DateTime.Now;
                string nuevoId = $"{ahora:yyyyMMddHHmmssfff}";
                string archivoIntercambio = $"{origen.EmisorId}.{origen.ReceptorId}.{nuevoId}{Path.GetExtension(origen.ArchivoIntercambio)}";
                string path0 = Path.Join(origen.RutaArchivo, DirectorioRespaldo, origen.ArchivoIntercambio);
                string path1 = Path.Join(origen.RutaArchivo, DirectorioRespaldo, archivoIntercambio);
                string path2 = Path.Join(origen.RutaArchivo, DirectorioEntradas, archivoIntercambio);
                string? drct1 = Path.GetDirectoryName(path1);

                try
                {
                    if (!Directory.Exists(drct1!))
                    {
                        Directory.CreateDirectory(drct1!);
                        helper.Audit([$"Directorio {drct1!} creado"]);
                    }
                    else
                    {
                        drct1 = null;
                    }
                    try
                    {
                        System.IO.File.Copy(path0, path1);
                        System.IO.File.SetCreationTime(path1, ahora);
                        System.IO.File.SetLastWriteTime(path1, ahora);
                        helper.Audit([$"Entrada {path1} creada"]);
                        try
                        {
                            System.IO.File.Copy(path0, path2);
                            System.IO.File.SetCreationTime(path2, ahora);
                            System.IO.File.SetLastWriteTime(path2, ahora);
                            helper.Audit([$"Respaldo {path2} creado"]);
                            try
                            {
                                _context.Add(new Intercambio
                                {
                                    Id = nuevoId,
                                    Cargado = ahora,
                                    Descargado = null,
                                    Numero = origen.Numero,
                                    TipoIntercambio = origen.TipoIntercambio,
                                    EmisorId = origen.EmisorId,
                                    ReceptorId = origen.ReceptorId,
                                    TipoDocumento = origen.TipoDocumento,
                                    ArchivoOriginal = origen.ArchivoOriginal,
                                    ArchivoIntercambio = archivoIntercambio,
                                    RutaArchivo = origen.RutaArchivo,
                                    Tamano = origen.Tamano,
                                    Status = Status.DISPONIBLE,
                                    EsRecarga = true
                                });
                                await _context.SaveChangesAsync();
                                helper.Audit([$"{id} recargado como {nuevoId}"]);
                            }
                            catch (Exception e)
                            {
                                if (await _context.Intercambios.FindAsync(nuevoId) is not null)
                                {
                                    helper.Audit([$"Error tras recargar {id} como {nuevoId}; esta recarga no será revertida", e.Message]);
                                }
                                else
                                {
                                    helper.Audit([mensaje = $"{id} no recargado como {nuevoId}"]);
                                    throw;
                                }
                            }
                        }
                        catch
                        {
                            if (System.IO.File.Exists(path2))
                            {
                                System.IO.File.Delete(path2);
                                helper.Audit([$"Respaldo {path2} removido"]);
                            }
                            throw;
                        }
                    }
                    catch
                    {
                        if (System.IO.File.Exists(path1))
                        {
                            System.IO.File.Delete(path1);
                            helper.Audit([$"Entrada {path1} removida"]);
                        }
                        throw;
                    }
                }
                catch (Exception e)
                {
                    if (drct1 is not null && Directory.Exists(drct1))
                    {
                        Directory.Delete(drct1, true);
                        helper.Audit([$"Directorio {drct1} removido"]);
                    }
                    helper.Audit([e.Message]);
                }
            }
            else
            {
                helper.Audit([mensaje = $"Intercambio {id} no encontrado"]);
            }
            await _context.SaveChangesAsync();
        }
        catch (Exception e)
        {
            helper.Audit(e switch
            {
                OperationCanceledException or DbUpdateException => [mensaje = "Error actualizando base de datos", e.Message],
                _ => [mensaje = $"Error revirtiendo recarga de {id}, e.Message"]
            });
        }
        return RedirectToAction("Intercambios", new { MostrarEntradas = mostrarEntradas, EDIActual = User.IsInRole("User") switch { true => null, false => ediActual }, Desde = $"{desde:yyyy-MM-dd}", Hasta = $"{hasta:yyyy-MM-dd}", Talla = talla, Pagina = pagina, Mensaje = mensaje });
    }

    // POST: Monitor/Anular
    [HttpPost, ValidateAntiForgeryToken]
    [Authorize(Roles = "User,Admin")]
    public async Task<IActionResult> Anular(bool mostrarEntradas, string ediActual, DateTime desde, DateTime hasta, int talla, int pagina, string id)
    {
        Helper<MonitorController> helper = new(_context, (await _userManager.GetUserAsync(User))!, HttpContext);
        string? mensaje = null;

        try
        {
            if (await _context.Intercambios.FindAsync(id) is Intercambio origen)
            {
                DateTime ahora = DateTime.Now;
                string path0 = Path.Join(origen.RutaArchivo, DirectorioEntradas, origen.ArchivoIntercambio);
                string path1 = Path.Join(origen.RutaArchivo, DirectorioEntradas, $"@{origen.ArchivoIntercambio}");

                try
                {
                    System.IO.File.Move(path0, path1);
                    helper.Audit([$"Entrada {path0} eliminada"]);
                    try
                    {
                        origen.Descargado = ahora;
                        origen.Status = Status.ANULADO;
                        await _context.SaveChangesAsync();
                        helper.Audit([$"{id} cambia a estado {Status.ANULADO}"]);
                    }
                    catch (Exception e)
                    {
                        if (await _context.Intercambios.FindAsync(id) is Intercambio { Status: Status.ANULADO })
                        {
                            helper.Audit([$"Error tras anular {id}; esta anulación no será revertida", e.Message]);
                        }
                        else
                        {
                            helper.Audit([mensaje = $"{id} no anulado"]);
                            throw;
                        }
                    }
                }
                catch (Exception e)
                {
                    if (System.IO.File.Exists(path1))
                    {
                        System.IO.File.Move(path1, path0);
                    }
                    helper.Audit([$"Entrada {path0} no eliminada", e.Message]);
                }
                if (System.IO.File.Exists(path1))
                {
                    System.IO.File.Delete(path1);
                }
            }
            else
            {
                helper.Audit([mensaje = $"Intercambio {id} no encontrado"]);
            }
            await _context.SaveChangesAsync();
        }
        catch (Exception e)
        {
            helper.Audit(e switch
            {
                OperationCanceledException or DbUpdateException => [mensaje = "Error actualizando base de datos", e.Message],
                _ => [mensaje = $"Error revirtiendo anulación de {id}, e.Message"]
            });
        }
        return RedirectToAction("Intercambios", new { MostrarEntradas = mostrarEntradas, EDIActual = User.IsInRole("User") switch { true => null, false => ediActual }, Desde = $"{desde:yyyy-MM-dd}", Hasta = $"{hasta:yyyy-MM-dd}", Talla = talla, Pagina = pagina, Mensaje = mensaje });
    }

    private const string DirectorioRespaldo = "RESPALDO";
    private const string DirectorioEntradas = "ENTRADAS";
}
