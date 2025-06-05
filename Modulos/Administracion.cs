using Spectre.Console;
using System.IO.Compression;
using System.Net.Mail;
using System.Net;
using Microsoft.Data.Sqlite;
using DotNetEnv;

namespace Modulos;

public static class GestionAdministracion
{
    private static readonly string rutaBD = Env.GetString("DB_PATH", "sistema.db");
    private static readonly string CarpetaBackups = Path.Combine(Directory.GetCurrentDirectory(), "Backups");
    private static readonly string CorreoAdmin = Env.GetString("CORREO_ADMIN", "admin@ejemplo.com");

    public static void Ejecutar()
    {
        Env.Load(); // Cargar .env al iniciar
        while (true)
        {
            var opcion = AnsiConsole.Prompt(
                new SelectionPrompt<string>()
                    .Title("Menú de Administración del Sistema")
                    .AddChoices([
                        "Backup y Restauración",
                        "Auditoría / Registro de Actividades",
                        "Limpieza y Mantenimiento",
                        "Pruebas del Sistema / Diagnóstico",
                        "Volver al menú principal"
                    ]));

            switch (opcion)
            {
                case "Backup y Restauración":
                    MenuBackupRestauracion();
                    break;
                case "Auditoría / Registro de Actividades":
                    MenuAuditoria();
                    break;
                case "Limpieza y Mantenimiento":
                    MenuLimpieza();
                    break;
                case "Pruebas del Sistema / Diagnóstico":
                    MenuDiagnostico();
                    break;
                case "Volver al menú principal":
                    return;
            }
        }
    }

    private static void MenuBackupRestauracion()
    {
        while (true)
        {
            var opcion = AnsiConsole.Prompt(
                new SelectionPrompt<string>()
                    .Title("Backup y Restauración")
                    .AddChoices([
                        "Crear copia de seguridad",
                        "Restaurar desde archivo",
                        "Ver historial de backups",
                        "Volver"
                    ]));

            switch (opcion)
            {
                case "Crear copia de seguridad":
                    CrearBackup();
                    break;
                case "Restaurar desde archivo":
                    RestaurarBackup();
                    break;
                case "Ver historial de backups":
                    MostrarHistorialBackups();
                    break;
                case "Volver":
                    return;
            }
        }
    }

    static void MenuAuditoria() => AnsiConsole.MarkupLine("[green]Función para mostrar logs del sistema (pendiente).[/]");
    static void MenuLimpieza() => AnsiConsole.MarkupLine("[green]Función para limpieza y eliminación de registros antiguos (pendiente).[/]");
    static void MenuDiagnostico() => AnsiConsole.MarkupLine("[green]Función para diagnósticos y pruebas del sistema (pendiente).[/]");

    private static void CrearBackup()
    {
        try
        {
            Directory.CreateDirectory(CarpetaBackups);

            string fecha = DateTime.Now.ToString("yyyyMMdd_HHmmss");
            string archivoTemporal = Path.Combine(CarpetaBackups, $"temp_sistema_{fecha}.db");
            string archivoZip = Path.Combine(CarpetaBackups, $"backup_{fecha}.zip");

            File.Copy(rutaBD, archivoTemporal, true);

            using (var zip = ZipFile.Open(archivoZip, ZipArchiveMode.Create))
            {
                zip.CreateEntryFromFile(archivoTemporal, Path.Combine("BackupContenido", "sistema.db"));
            }

            File.Delete(archivoTemporal);

            AnsiConsole.MarkupLine($"[green]Backup creado correctamente:[/] {archivoZip}");

            _ = EnviarCorreo(CorreoAdmin, archivoZip);
            AnsiConsole.MarkupLine($"[green]Backup enviado a {CorreoAdmin} correctamente.[/]");
        }
        catch (Exception ex)
        {
            AnsiConsole.MarkupLine($"[red]Error al crear backup:[/] {ex.Message}");
        }
    }

    private static void RestaurarBackup()
    {
        try
        {
            if (!Directory.Exists(CarpetaBackups))
            {
                AnsiConsole.MarkupLine("[red]No hay carpeta de backups creada.[/]");
                return;
            }

            var archivos = Directory.GetFiles(CarpetaBackups, "*.zip").OrderByDescending(File.GetCreationTime).ToList();
            if (archivos.Count == 0)
            {
                AnsiConsole.MarkupLine("[red]No hay archivos de backup disponibles.[/]");
                return;
            }

            var seleccion = AnsiConsole.Prompt(
                new SelectionPrompt<string>()
                    .Title("[yellow]Seleccione un backup para restaurar:[/]")
                    .AddChoices(archivos.Select(Path.GetFileName)));

            string rutaSeleccionada = Path.Combine(CarpetaBackups, seleccion);
            string carpetaTemporal = Path.Combine(CarpetaBackups, "TempRestore");
            if (Directory.Exists(carpetaTemporal)) Directory.Delete(carpetaTemporal, true);

            ZipFile.ExtractToDirectory(rutaSeleccionada, carpetaTemporal);

            string archivoExtraido = Path.Combine(carpetaTemporal, "BackupContenido", "sistema.db");

            if (!File.Exists(archivoExtraido))
            {
                AnsiConsole.MarkupLine("[red]El archivo de base de datos no se encontró dentro del backup.[/]");
                return;
            }

            File.Copy(archivoExtraido, rutaBD, true);
            Directory.Delete(carpetaTemporal, true);

            AnsiConsole.MarkupLine($"[green]Restauración completada desde:[/] {rutaSeleccionada}");
        }
        catch (Exception ex)
        {
            AnsiConsole.MarkupLine($"[red]Error al restaurar backup:[/] {ex.Message}");
        }
    }

    private static void MostrarHistorialBackups()
    {
        try
        {
            if (!Directory.Exists(CarpetaBackups))
            {
                AnsiConsole.MarkupLine("[red]No hay carpeta de backups creada.[/]");
                return;
            }

            var archivos = Directory.GetFiles(CarpetaBackups, "*.zip").OrderByDescending(File.GetCreationTime).ToList();
            if (archivos.Count == 0)
            {
                AnsiConsole.MarkupLine("[red]No hay backups registrados.[/]");
                return;
            }

            var tabla = new Table().Title("[green]Historial de Backups[/]");
            tabla.AddColumn("Archivo");
            tabla.AddColumn("Fecha de Creación");

            foreach (var archivo in archivos)
            {
                var info = new FileInfo(archivo);
                tabla.AddRow(info.Name, info.CreationTime.ToString("g"));
            }

            AnsiConsole.Write(tabla);
        }
        catch (Exception ex)
        {
            AnsiConsole.MarkupLine($"[red]Error al mostrar historial:[/] {ex.Message}");
        }
    }

    private static async Task EnviarCorreo(string destinatario, string archivoRespaldo)
    {
        string smtpUser = Env.GetString("SMTP_USER");
        string smtpPass = Env.GetString("SMTP_PASS");
        string smtpHost = Env.GetString("SMTP_HOST", "smtp.gmail.com");
        int smtpPort = int.Parse(Env.GetString("SMTP_PORT", "587"));

        var mail = new MailMessage(smtpUser, destinatario, "Backup del sistema", "Adjunto el archivo con el respaldo de la base de datos.");
        mail.Attachments.Add(new Attachment(archivoRespaldo));

        using var client = new SmtpClient(smtpHost, smtpPort)
        {
            Credentials = new NetworkCredential(smtpUser, smtpPass),
            EnableSsl = true
        };

        await client.SendMailAsync(mail);
    }
}
