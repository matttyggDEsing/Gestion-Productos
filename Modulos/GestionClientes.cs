using Spectre.Console;
using Modelos;
using Datos;
using DotNetEnv;

namespace Modulos;

public static class GestionClientes
{
    public static void Ejecutar()
    {
        while (true)
        {
            var opcion = AnsiConsole.Prompt(
                new SelectionPrompt<string>()
                    .Title("[yellow]Gestión de Clientes[/]")
                    .AddChoices("Agregar Cliente", "Mostrar Clientes", "Eliminar Cliente", "Volver"));

            switch (opcion)
            {
                case "Agregar Cliente":
                    var cliente = new Cliente
                    {
                        Nombre = AnsiConsole.Ask<string>("Nombre del cliente:"),
                        Contacto = AnsiConsole.Ask<string>("Teléfono o contacto:"),
                        Email = AnsiConsole.Ask<string>("Email:"),
                        Direccion = AnsiConsole.Ask<string>("Dirección:"),
                        Ciudad = AnsiConsole.Ask<string>("Ciudad:")
                    };

                    BaseDatos.AgregarCliente(cliente);
                    AnsiConsole.MarkupLine("[green]Cliente agregado correctamente.[/]");
                    break;

                case "Mostrar Clientes":
                    var clientes = BaseDatos.ObtenerClientes();

                    if (clientes.Count == 0)
                    {
                        AnsiConsole.MarkupLine("[red]No hay clientes registrados.[/]");
                        break;
                    }

                    var tabla = new Table().Border(TableBorder.Rounded);
                    tabla.AddColumns("Nombre", "Contacto", "Email", "Dirección", "Ciudad");

                    foreach (var c in clientes)
                        tabla.AddRow(c.Nombre, c.Contacto, c.Email, c.Direccion, c.Ciudad);

                    AnsiConsole.Write(tabla);
                    break;

                case "Eliminar Cliente":
                    var clientesEliminar = BaseDatos.ObtenerClientes();
                    if (clientesEliminar.Count == 0)
                    {
                        AnsiConsole.MarkupLine("[red]No hay clientes registrados.[/]");
                        break;
                    }

                    var nombres = clientesEliminar.Select(c => c.Nombre).ToList();

                    var nombreEliminar = AnsiConsole.Prompt(
                        new SelectionPrompt<string>()
                            .Title("Seleccione un cliente para eliminar:")
                            .AddChoices(nombres));

                    BaseDatos.EliminarCliente(nombreEliminar);
                    AnsiConsole.MarkupLine($"[red]Cliente eliminado:[/] {nombreEliminar}");
                    break;

                case "Volver":
                    return;
            }
        }
    }
}
