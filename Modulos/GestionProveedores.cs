using Modelos;
using Spectre.Console;
using Datos;
using DotNetEnv;

namespace Modulos;

public static class GestionProveedores
{
    public static void Ejecutar()
    {
        var opciones = new[] { "Agregar Proveedor", "Mostrar Proveedores", "Eliminar Proveedor", "Volver" };
        var seleccion = AnsiConsole.Prompt(
            new SelectionPrompt<string>()
                .Title("[yellow]Gestión de Proveedores:[/]")
                .AddChoices(opciones));

        switch (seleccion)
        {
            case "Agregar Proveedor":
                var nuevo = new Proveedor
                {
                    Nombre = AnsiConsole.Ask<string>("Nombre del proveedor:"),
                    Contacto = AnsiConsole.Ask<string>("Contacto del proveedor:"),
                    Email = AnsiConsole.Ask<string>("Email del proveedor:")
                };
                BaseDatos.AgregarProveedor(nuevo);
                AnsiConsole.MarkupLine("[green]Proveedor agregado correctamente.[/]");
                break;

            case "Mostrar Proveedores":
                var proveedores = BaseDatos.ObtenerProveedores();

                if (proveedores.Count == 0)
                {
                    AnsiConsole.MarkupLine("[red]No hay proveedores registrados.[/]");
                    return;
                }

                var table = new Table().Border(TableBorder.Rounded);
                table.AddColumn("Nombre");
                table.AddColumn("Contacto");
                table.AddColumn("Email");

                foreach (var p in proveedores)
                    table.AddRow(p.Nombre, p.Contacto ?? "-", p.Email ?? "-");

                AnsiConsole.Write(table);
                break;

            case "Eliminar Proveedor":
                var todos = BaseDatos.ObtenerProveedores();

                if (todos.Count == 0)
                {
                    AnsiConsole.MarkupLine("[red]No hay proveedores registrados.[/]");
                    return;
                }

                var nombre = AnsiConsole.Prompt(
                    new SelectionPrompt<string>()
                        .Title("Seleccione el proveedor a eliminar:")
                        .AddChoices(todos.Select(p => p.Nombre)));

                if (BaseDatos.ProveedorTieneProductos(nombre))
                {
                    AnsiConsole.MarkupLine($"[red]No se puede eliminar el proveedor '{nombre}' porque tiene productos asociados.[/]");
                }
                else
                {
                    BaseDatos.EliminarProveedor(nombre);
                    AnsiConsole.MarkupLine("[green]Proveedor eliminado correctamente.[/]");
                }
                break;
        }
    }
}
