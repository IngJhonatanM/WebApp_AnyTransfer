using System.ComponentModel.DataAnnotations;

namespace EDIBANK.Models.Monitor;

public enum TipoIntercambio : byte
{
    [Display(Name = "Desconocido")]
    DESCONOCIDO,
    [Display(Name = "EDIFACT")]
    EDIFACT,
    [Display(Name = "CIPHER")]
    CIPHER
}

public enum Status : byte
{
    [Display(Name = "Rechazado")]
    RECHAZADO,
    [Display(Name = "Disponible")]
    DISPONIBLE,
    [Display(Name = "Descargado")]
    DESCARGADO,
    [Display(Name = "Anulado")]
    ANULADO
}
