using Spectre.Console;
using Modelos;
using Modulos;
using Datos;
using System.Net;
using System.Net.Mail;
using DotNetEnv;


List<Cliente> clientes = new();
List<Pedido> pedidos = new();
BaseDatos.Inicializar();
var productos = BaseDatos.ObtenerProductos();
var proveedores = BaseDatos.ObtenerProveedores();
while (true)
{
    Console.Clear();
    var opcion = AnsiConsole.Prompt(
        new SelectionPrompt<string>()
            .Title("[bold yellow]Menú Principal - Sistema de Pedidos[/]")
            .PageSize(10)
            .AddChoices(new[]
            {
                "Gestión de Productos",
                "Gestión de Clientes",
                "Gestión de Pedidos",
                "Pagos y Facturación",
                "Reportes e Informes",
                "Administración del Sistema",
                "Gestión de Proveedores",
                "Salir"
            }));

    Console.Clear();
    switch (opcion)
    {
        case "Gestión de Productos":
            GestionProductos.Ejecutar();
            break;
        case "Gestión de Clientes":
            GestionClientes.Ejecutar();
            break;
        case "Gestión de Pedidos":
            GestionPedidos.Ejecutar();
            break;
        case "Pagos y Facturación":
            GestionFacturacion.Ejecutar();
            break;
        case "Reportes e Informes":
            GestionReportes.Ejecutar();
            break;
        case "Administración del Sistema":
            GestionAdministracion.Ejecutar();
            break;
        case "Gestión de Proveedores":
            GestionProveedores.Ejecutar();
            break;
        case "Salir":
            AnsiConsole.MarkupLine("[green]Gracias por usar el sistema. ¡Hasta luego![/]");
            return;
    }

    AnsiConsole.MarkupLine("\n[grey]Presione una tecla para continuar...[/]");
    Console.ReadKey();
}
