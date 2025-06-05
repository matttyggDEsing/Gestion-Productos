using Modelos;
using Spectre.Console;
using System.Net;
using System.Net.Mail;
using Datos;
using DotNetEnv;

namespace Modulos;

public static class GestionFacturacion
{
    public static async Task Ejecutar()
    {
        Env.Load(); // Carga variables del archivo .env

        var clientes = BaseDatos.ObtenerClientes();

        if (clientes.Count == 0)
        {
            AnsiConsole.MarkupLine("[red]No hay clientes registrados.[/]");
            return;
        }

        var clienteSeleccionado = AnsiConsole.Prompt(
            new SelectionPrompt<string>()
                .Title("[yellow]Seleccione un cliente para facturar:[/]")
                .AddChoices(clientes.Select(c => c.Nombre)));

        var cliente = clientes.First(c => c.Nombre == clienteSeleccionado);
        var pedidos = BaseDatos.ObtenerPedidosPorCliente(cliente.Id);

        if (pedidos.Count == 0)
        {
            AnsiConsole.MarkupLine("[red]Este cliente no tiene pedidos para facturar.[/]");
            return;
        }

        var total = pedidos.Sum(p => p.Cantidad * p.Producto.Precio);
        var factura = new Factura
        {
            Cliente = cliente.Nombre,
            Fecha = DateTime.Now,
            Total = total
        };

        var detalles = pedidos.Select(p => new DetalleFactura
        {
            Producto = p.Producto.Nombre,
            Cantidad = p.Cantidad,
            PrecioUnitario = p.Producto.Precio
        }).ToList();

        // Guardar factura y eliminar pedidos facturados
        BaseDatos.AgregarFacturaConDetalles(factura, detalles, cliente.Id);

        // Mostrar factura en pantalla
        var textoFactura = $"""
        Factura para: {cliente.Nombre}
        Dirección: {cliente.Direccion}
        Ciudad: {cliente.Ciudad}
        -----------------------------
        {string.Join(Environment.NewLine, pedidos.Select(p => $"{p.Cantidad}x {p.Producto.Nombre} - ${p.Producto.Precio:F2}"))}
        -----------------------------
        Total a pagar: ${total:F2}
        Fecha: {DateTime.Now}
        """;

        var nombreArchivo = $"Factura_{cliente.Nombre}_{DateTime.Now:yyyyMMddHHmmss}.txt";
        File.WriteAllText(nombreArchivo, textoFactura);

        AnsiConsole.MarkupLine("[green]Factura generada:[/]");
        AnsiConsole.WriteLine(textoFactura);

        var enviar = AnsiConsole.Confirm("¿Desea enviar esta factura por correo electrónico al cliente?");

        if (enviar)
        {
            try
            {
                await EnviarCorreo(cliente.Email!, nombreArchivo);
                AnsiConsole.MarkupLine($"[green]Factura enviada a {cliente.Email} correctamente.[/]");
            }
            catch (Exception ex)
            {
                AnsiConsole.MarkupLine($"[red]Error al enviar el correo: {ex.Message}[/]");
            }
        }
    }

    static async Task EnviarCorreo(string destinatario, string archivoRespaldo)
    {
        // Cargar configuración desde .env
        var email = Env.GetString("EMAIL_USER");
        var pass = Env.GetString("EMAIL_PASS");
        var host = Env.GetString("SMTP_HOST") ?? "smtp.gmail.com";
        var puerto = int.Parse(Env.GetString("SMTP_PORT") ?? "587");

        var mail = new MailMessage(email, destinatario, "Factura de Pedido", "Adjunto archivo con la factura de su pedido.");
        mail.Attachments.Add(new Attachment(archivoRespaldo));

        using var client = new SmtpClient(host, puerto)
        {
            Credentials = new NetworkCredential(email, pass),
            EnableSsl = true
        };

        await client.SendMailAsync(mail);
    }
}
