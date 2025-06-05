using Modelos;
using Spectre.Console;
using Datos;
using DotNetEnv;

namespace Modulos;

public static class GestionProductos
{
    public static void Ejecutar()
    {
        var opciones = new[] { "Agregar Producto", "Mostrar Productos", "Eliminar Producto", "Volver" };
        var seleccion = AnsiConsole.Prompt(
            new SelectionPrompt<string>()
                .Title("[yellow]Gestión de Productos:[/]")
                .AddChoices(opciones));

        switch (seleccion)
        {
            case "Agregar Producto":
                var proveedores = BaseDatos.ObtenerProveedores();

                string proveedorSeleccionado;

                if (proveedores.Count == 0)
                {
                    AnsiConsole.MarkupLine("[yellow]No hay proveedores registrados. Ingresa uno nuevo:[/]");
                    proveedorSeleccionado = AnsiConsole.Ask<string>("Nombre del proveedor:");
                    var nuevoProveedor = new Proveedor
                    {
                        Nombre = proveedorSeleccionado,
                        Contacto = AnsiConsole.Ask<string>("Contacto del proveedor:"),
                        Email = AnsiConsole.Ask<string>("Email del proveedor:")
                    };
                    BaseDatos.AgregarProveedor(nuevoProveedor);
                }
                else
                {
                    var opcionesProveedor = proveedores.Select(p => p.Nombre).ToList();
                    opcionesProveedor.Add("Nuevo proveedor");

                    proveedorSeleccionado = AnsiConsole.Prompt(
                        new SelectionPrompt<string>()
                            .Title("Selecciona un [green]proveedor[/] o elige 'Nuevo proveedor':")
                            .AddChoices(opcionesProveedor)
                    );

                    if (proveedorSeleccionado == "Nuevo proveedor")
                    {
                        proveedorSeleccionado = AnsiConsole.Ask<string>("Nombre del nuevo proveedor:");
                        var nuevoProveedor = new Proveedor
                        {
                            Nombre = proveedorSeleccionado,
                            Contacto = AnsiConsole.Ask<string>("Contacto del nuevo proveedor:"),
                            Email = AnsiConsole.Ask<string>("Email del nuevo proveedor:")
                        };
                        BaseDatos.AgregarProveedor(nuevoProveedor);
                        AnsiConsole.MarkupLine("[green]Nuevo proveedor agregado.[/]");
                    }
                }

                var producto = new Producto
                {
                    Nombre = AnsiConsole.Ask<string>("Nombre del producto:"),
                    Descripcion = AnsiConsole.Ask<string>("Descripción del producto:"),
                    Precio = AnsiConsole.Ask<double>("Precio del producto:"),
                    Stock = AnsiConsole.Ask<int>("Cantidad en stock:"),
                    Proveedor = proveedorSeleccionado
                };

                BaseDatos.AgregarProducto(producto);
                AnsiConsole.MarkupLine("[green]Producto agregado correctamente.[/]");
                break;

            case "Mostrar Productos":
                var productos = BaseDatos.ObtenerProductos();

                if (productos.Count == 0)
                {
                    AnsiConsole.MarkupLine("[red]No hay productos registrados.[/]");
                    return;
                }

                var table = new Table().Border(TableBorder.Rounded);
                table.AddColumn("Nombre");
                table.AddColumn("Descripción");
                table.AddColumn("Proveedor");
                table.AddColumn("Precio");
                table.AddColumn("Stock");

                foreach (var p in productos)
                {
                    table.AddRow(
                        p.Nombre,
                        p.Descripcion ?? "-",
                        p.Proveedor ?? "-",
                        $"${p.Precio:F2}",
                        p.Stock.ToString()
                    );
                }

                AnsiConsole.Write(table);
                break;

            case "Eliminar Producto":
                var todos = BaseDatos.ObtenerProductos();

                if (todos.Count == 0)
                {
                    AnsiConsole.MarkupLine("[red]No hay productos registrados.[/]");
                    return;
                }

                var nombre = AnsiConsole.Prompt(
                    new SelectionPrompt<string>()
                        .Title("[yellow]Seleccione el producto a eliminar:[/]")
                        .AddChoices(todos.Select(p => p.Nombre)));

                BaseDatos.EliminarProducto(nombre);
                AnsiConsole.MarkupLine("[green]Producto eliminado correctamente.[/]");
                break;
        }
    }
}
