using Datos;
using Spectre.Console;
using DotNetEnv;

public static class GestionReportes
{
    public static void Ejecutar()
    {
        var facturas = BaseDatos.ObtenerFacturas();

        if (facturas.Count == 0)
        {
            AnsiConsole.MarkupLine("[red]No hay facturas registradas.[/]");
            return;
        }

        var clienteSeleccionado = AnsiConsole.Prompt(
            new SelectionPrompt<string>()
                .Title("[yellow]Seleccione un cliente:[/]")
                .AddChoices(facturas.Select(f => f.Cliente).Distinct()));

        var facturasCliente = facturas.Where(f => f.Cliente == clienteSeleccionado).ToList();

        var opciones = facturasCliente.Select(f =>
            $"ID: {f.Id} | Fecha: {f.Fecha:g} | Total: ${f.Total:F2}").ToList();

        var seleccion = AnsiConsole.Prompt(
            new SelectionPrompt<string>()
                .Title($"[blue]Seleccione una factura de {clienteSeleccionado}:[/]")
                .AddChoices(opciones));

        var idSeleccionado = int.Parse(seleccion.Split('|')[0].Replace("ID:", "").Trim());
        var detalles = BaseDatos.ObtenerDetalleFactura(idSeleccionado);

        var tabla = new Table().Title($"[green]Detalle de Factura #{idSeleccionado}[/]");
        tabla.AddColumn("Producto");
        tabla.AddColumn("Cantidad");
        tabla.AddColumn("Precio Unitario");
        tabla.AddColumn("Subtotal");

        foreach (var d in detalles)
        {
            var subtotal = d.Cantidad * d.PrecioUnitario;
            tabla.AddRow(d.Producto, d.Cantidad.ToString(), $"${d.PrecioUnitario:F2}", $"${subtotal:F2}");
        }

        var total = detalles.Sum(d => d.Cantidad * d.PrecioUnitario);
        tabla.AddRow("[yellow]TOTAL[/]", "", "", $"[bold green]${total:F2}[/]");

        AnsiConsole.Write(tabla);

        // Confirmar eliminación
        var eliminar = AnsiConsole.Confirm("[red]¿Desea eliminar esta factura?[/]");

        if (eliminar)
        {
            BaseDatos.EliminarFactura(idSeleccionado);
            AnsiConsole.MarkupLine("[green]Factura eliminada correctamente.[/]");
        }
    }
}
