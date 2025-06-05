using Modelos;
using Spectre.Console;
using Datos;
using DotNetEnv;

namespace Modulos;

public static class GestionPedidos
{
    public static void Ejecutar()
    {
        while (true)
        {
            var opcion = AnsiConsole.Prompt(
                new SelectionPrompt<string>()
                    .Title("[yellow]Seleccione una opción de gestión de pedidos:[/]")
                    .AddChoices([
                        "Agregar pedido",
                        "Ver pedidos por cliente",
                        "Eliminar pedido",
                        "Volver al menú principal"
                    ]));

            switch (opcion)
            {
                case "Agregar pedido":
                    AgregarPedido();
                    break;
                case "Ver pedidos por cliente":
                    VerPedidosPorCliente();
                    break;
                case "Eliminar pedido":
                    EliminarPedido();
                    break;
                case "Volver al menú principal":
                    return;
            }
        }
    }

    private static void AgregarPedido()
    {
        var clientes = BaseDatos.ObtenerClientes();
        if (clientes.Count == 0)
        {
            AnsiConsole.MarkupLine("[red]No hay clientes registrados.[/]");
            return;
        }

        var clienteNombre = AnsiConsole.Prompt(
            new SelectionPrompt<string>()
                .Title("Seleccione un cliente:")
                .AddChoices(clientes.Select(c => c.Nombre)));
        var cliente = clientes.First(c => c.Nombre == clienteNombre);

        var productos = BaseDatos.ObtenerProductos();
        if (productos.Count == 0)
        {
            AnsiConsole.MarkupLine("[red]No hay productos disponibles.[/]");
            return;
        }

        var pedidos = new List<Pedido>();

        while (true)
        {
            var productoNombre = AnsiConsole.Prompt(
                new SelectionPrompt<string>()
                    .Title("Seleccione un producto:")
                    .AddChoices(productos.Select(p => p.Nombre)));
            var producto = productos.First(p => p.Nombre == productoNombre);

            int cantidad = AnsiConsole.Ask<int>($"Cantidad para [green]{producto.Nombre}[/] (Stock: {producto.Stock}):");

            if (cantidad > producto.Stock)
            {
                AnsiConsole.MarkupLine("[red]No hay suficiente stock disponible.[/]");
                continue;
            }

            pedidos.Add(new Pedido
            {
                Producto = producto,
                Cantidad = cantidad,
                ClienteId = cliente.Id,
                Cliente = cliente.Nombre
            });

            producto.Stock -= cantidad;
            BaseDatos.ActualizarStock(producto);

            if (!AnsiConsole.Confirm("¿Agregar otro producto al pedido?")) break;
        }

        foreach (var pedido in pedidos)
            BaseDatos.AgregarPedido(pedido);

        AnsiConsole.MarkupLine("[green]Pedido cargado correctamente.[/]");
    }

    private static void VerPedidosPorCliente()
    {
        var clientes = BaseDatos.ObtenerClientes();
        if (clientes.Count == 0)
        {
            AnsiConsole.MarkupLine("[red]No hay clientes registrados.[/]");
            return;
        }

        var clienteNombre = AnsiConsole.Prompt(
            new SelectionPrompt<string>()
                .Title("Seleccione un cliente:")
                .AddChoices(clientes.Select(c => c.Nombre)));

        var cliente = clientes.First(c => c.Nombre == clienteNombre);
        var pedidos = BaseDatos.ObtenerPedidosPorCliente(cliente.Id);

        if (pedidos.Count == 0)
        {
            AnsiConsole.MarkupLine("[red]Este cliente no tiene pedidos registrados.[/]");
            return;
        }

        var tabla = new Table().Title($"Pedidos de {cliente.Nombre}")
            .AddColumn("Producto")
            .AddColumn("Cantidad")
            .AddColumn("Precio")
            .AddColumn("Total");

        foreach (var pedido in pedidos)
        {
            var total = pedido.Cantidad * pedido.Producto.Precio;
            tabla.AddRow(pedido.Producto.Nombre, pedido.Cantidad.ToString(), pedido.Producto.Precio.ToString("F2"), total.ToString("F2"));
        }

        AnsiConsole.Write(tabla);
    }

    private static void EliminarPedido()
    {
        var pedidos = BaseDatos.ObtenerPedidos();
        if (pedidos.Count == 0)
        {
            AnsiConsole.MarkupLine("[red]No hay pedidos registrados.[/]");
            return;
        }

        var opciones = pedidos.Select(p => $"{p.IdPedido} - {p.Cliente}: {p.Producto.Nombre} x{p.Cantidad}").ToList();
        var seleccion = AnsiConsole.Prompt(
            new SelectionPrompt<string>()
                .Title("Seleccione un pedido para eliminar:")
                .AddChoices(opciones));

        var id = int.Parse(seleccion.Split(' ')[0]);
        BaseDatos.EliminarPedido(id);
        AnsiConsole.MarkupLine("[green]Pedido eliminado correctamente.[/]");
    }
}
