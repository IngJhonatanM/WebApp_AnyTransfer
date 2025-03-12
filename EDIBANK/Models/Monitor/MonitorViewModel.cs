// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
using System.ComponentModel.DataAnnotations;

namespace EDIBANK.Models.Monitor;

public class MonitorViewModel
{
    public required bool MostrarEntradas { get; init; }

    public required string EDIActual { get; init; }

    public required string Menor { get; init; }

    public required string Mayor { get; init; }

    [DataType(DataType.Date)]
    [Display(Name = "Desde")]
    public required DateTime Desde { get; init; }

    [DataType(DataType.Date)]
    [Display(Name = "Hasta")]
    public required DateTime Hasta { get; init; }

    public required int Talla { get; init; }

    public required int Pagina { get; init; }

    public required int TotalPaginas { get; init; }

    public required IEnumerable<Intercambio> Intercambios { get; init; }
}
