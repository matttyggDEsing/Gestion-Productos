using Microsoft.Data.Sqlite;
using System.IO;
using Spectre.Console;
using Modelos;
using DotNetEnv;
namespace Datos;

public static class BaseDatos
{
    private static readonly string rutaBD;

    static BaseDatos()
    {
        Env.Load(); // Carga variables del .env
        rutaBD = Env.GetString("DB_PATH") ?? "sistema.db";
    }

    public static void Inicializar()
    {
        using var conexion = new SqliteConnection($"Data Source={rutaBD}");
        conexion.Open();

        var comandos = new[]
        {
            """
            CREATE TABLE IF NOT EXISTS Productos (
                Id INTEGER PRIMARY KEY AUTOINCREMENT,
                Nombre TEXT,
                Descripcion TEXT,
                Proveedor TEXT,
                Precio REAL,
                Stock INTEGER
            );
            """,
            """
            CREATE TABLE IF NOT EXISTS Clientes (
                Id INTEGER PRIMARY KEY AUTOINCREMENT,
                Nombre TEXT,
                Telefono TEXT,
                Email TEXT,
                Direccion TEXT,
                Ciudad TEXT
            );
            """,
            """
            CREATE TABLE IF NOT EXISTS Pedidos (
                IdPedido INTEGER PRIMARY KEY AUTOINCREMENT,
                Producto TEXT NOT NULL,
                ClienteId INTEGER NOT NULL,
                Cantidad INTEGER NOT NULL
            );
            """,
            """
            CREATE TABLE IF NOT EXISTS Proveedores (
                Id INTEGER PRIMARY KEY AUTOINCREMENT,
                Nombre TEXT,
                Contacto TEXT,
                Email TEXT
            );
            """,
            """
            CREATE TABLE IF NOT EXISTS Facturas (
                Id INTEGER PRIMARY KEY AUTOINCREMENT,
                Cliente TEXT,
                Fecha TEXT,
                Total REAL
            );
            """,
            """
            CREATE TABLE IF NOT EXISTS DetalleFactura (
                Id INTEGER PRIMARY KEY AUTOINCREMENT,
                IdFactura INTEGER NOT NULL,
                Producto TEXT NOT NULL,
                Cantidad INTEGER NOT NULL,
                PrecioUnitario REAL NOT NULL,
                FOREIGN KEY (IdFactura) REFERENCES Facturas(Id) ON DELETE CASCADE
            );
            """

        };

        foreach (var cmdText in comandos)
        {
            using var cmd = conexion.CreateCommand();
            cmd.CommandText = cmdText;
            cmd.ExecuteNonQuery();
        }

        conexion.Close();
    }

    public static SqliteConnection ObtenerConexion() =>
        new SqliteConnection($"Data Source={rutaBD}");

    public static void AgregarProducto(Producto producto)
    {
        using var conexion = ObtenerConexion();
        conexion.Open();

        using var cmd = conexion.CreateCommand();
        cmd.CommandText = """
        INSERT INTO Productos (Nombre, Descripcion, Proveedor, Precio, Stock)
        VALUES ($nombre, $descripcion, $proveedor, $precio, $stock)
    """;

        cmd.Parameters.AddWithValue("$nombre", producto.Nombre);
        cmd.Parameters.AddWithValue("$descripcion", producto.Descripcion ?? "");
        cmd.Parameters.AddWithValue("$proveedor", producto.Proveedor ?? "");
        cmd.Parameters.AddWithValue("$precio", producto.Precio);
        cmd.Parameters.AddWithValue("$stock", producto.Stock);

        cmd.ExecuteNonQuery();
    }
    public static void ActualizarProducto(Producto producto)
    {
        using var conexion = ObtenerConexion();
        conexion.Open();

        using var cmd = conexion.CreateCommand();
        cmd.CommandText = """
        UPDATE Productos
        SET Descripcion = $descripcion,
            Proveedor = $proveedor,
            Precio = $precio,
            Stock = $stock
        WHERE Nombre = $nombre
    """;

        cmd.Parameters.AddWithValue("$nombre", producto.Nombre);
        cmd.Parameters.AddWithValue("$descripcion", producto.Descripcion ?? "");
        cmd.Parameters.AddWithValue("$proveedor", producto.Proveedor ?? "");
        cmd.Parameters.AddWithValue("$precio", producto.Precio);
        cmd.Parameters.AddWithValue("$stock", producto.Stock);

        cmd.ExecuteNonQuery();
    }
    public static void EliminarProducto(string nombre)
    {
        using var conexion = ObtenerConexion();
        conexion.Open();

        using var cmd = conexion.CreateCommand();
        cmd.CommandText = "DELETE FROM Productos WHERE Nombre = $nombre";
        cmd.Parameters.AddWithValue("$nombre", nombre);

        cmd.ExecuteNonQuery();
    }


    public static void AgregarProveedor(Proveedor proveedor)
    {
        using var conexion = ObtenerConexion();
        conexion.Open();

        // Evitar duplicados
        using var checkCmd = conexion.CreateCommand();
        checkCmd.CommandText = "SELECT COUNT(*) FROM Proveedores WHERE Nombre = $nombre";
        checkCmd.Parameters.AddWithValue("$nombre", proveedor.Nombre);
        var existe = Convert.ToInt32(checkCmd.ExecuteScalar()) > 0;

        if (existe) return;

        using var cmd = conexion.CreateCommand();
        cmd.CommandText = """
            INSERT INTO Proveedores (Nombre, Contacto, Email)
            VALUES ($nombre, $contacto, $email);
        """;
        cmd.Parameters.AddWithValue("$nombre", proveedor.Nombre);
        cmd.Parameters.AddWithValue("$contacto", proveedor.Contacto ?? "");
        cmd.Parameters.AddWithValue("$email", proveedor.Email ?? "");
        cmd.ExecuteNonQuery();
    }
    public static List<Producto> ObtenerProductos()
    {
        var productos = new List<Producto>();

        using var conexion = ObtenerConexion();
        conexion.Open();

        using var cmd = conexion.CreateCommand();
        cmd.CommandText = "SELECT Nombre, Descripcion, Proveedor, Precio, Stock FROM Productos";

        using var reader = cmd.ExecuteReader();
        while (reader.Read())
        {
            productos.Add(new Producto
            {
                Nombre = reader.GetString(0),
                Descripcion = reader.IsDBNull(1) ? null : reader.GetString(1),
                Proveedor = reader.IsDBNull(2) ? null : reader.GetString(2),
                Precio = reader.GetDouble(3),
                Stock = reader.GetInt32(4)
            });
        }

        return productos;
    }
    public static List<Proveedor> ObtenerProveedores()
    {
        var proveedores = new List<Proveedor>();

        using var conexion = ObtenerConexion();
        conexion.Open();

        using var cmd = conexion.CreateCommand();
        cmd.CommandText = "SELECT Nombre, Contacto, Email FROM Proveedores";

        using var reader = cmd.ExecuteReader();
        while (reader.Read())
        {
            proveedores.Add(new Proveedor
            {
                Nombre = reader.GetString(0),
                Contacto = reader.IsDBNull(1) ? null : reader.GetString(1),
                Email = reader.IsDBNull(2) ? null : reader.GetString(2)
            });
        }

        return proveedores;
    }
    public static void EliminarProveedor(string nombre)
    {
        if (ProveedorTieneProductos(nombre))
        {
            AnsiConsole.MarkupLine($"[red]No se puede eliminar el proveedor '{nombre}' porque tiene productos asociados.[/]");
            return;
        }

        using var conexion = ObtenerConexion();
        conexion.Open();

        using var cmd = conexion.CreateCommand();
        cmd.CommandText = "DELETE FROM Proveedores WHERE Nombre = $nombre";
        cmd.Parameters.AddWithValue("$nombre", nombre);

        cmd.ExecuteNonQuery();
    }


    public static bool ProveedorTieneProductos(string nombreProveedor)
    {
        using var conexion = ObtenerConexion();
        conexion.Open();

        using var cmd = conexion.CreateCommand();
        cmd.CommandText = "SELECT COUNT(*) FROM Productos WHERE Proveedor = $nombre";
        cmd.Parameters.AddWithValue("$nombre", nombreProveedor);

        return Convert.ToInt32(cmd.ExecuteScalar()) > 0;
    }


    public static void ActualizarProveedor(Proveedor proveedor)
    {
        using var conexion = ObtenerConexion();
        conexion.Open();

        using var cmd = conexion.CreateCommand();
        cmd.CommandText = """
        UPDATE Proveedores
        SET Contacto = $contacto,
            Email = $email
        WHERE Nombre = $nombre
    """;

        cmd.Parameters.AddWithValue("$nombre", proveedor.Nombre);
        cmd.Parameters.AddWithValue("$contacto", proveedor.Contacto ?? "");
        cmd.Parameters.AddWithValue("$email", proveedor.Email ?? "");

        cmd.ExecuteNonQuery();
    }
    public static void AgregarCliente(Cliente cliente)
    {
        using var conexion = ObtenerConexion();
        conexion.Open();

        using var cmd = conexion.CreateCommand();
        cmd.CommandText = """
        INSERT INTO Clientes (Nombre, Telefono, Email, Direccion, Ciudad)
        VALUES ($nombre, $telefono, $email, $direccion, $ciudad);
    """;

        cmd.Parameters.AddWithValue("$nombre", cliente.Nombre);
        cmd.Parameters.AddWithValue("$telefono", cliente.Contacto ?? "");
        cmd.Parameters.AddWithValue("$email", cliente.Email ?? "");
        cmd.Parameters.AddWithValue("$direccion", cliente.Direccion ?? "");
        cmd.Parameters.AddWithValue("$ciudad", cliente.Ciudad ?? "");

        cmd.ExecuteNonQuery();
    }

    public static List<Cliente> ObtenerClientes()
    {
        var clientes = new List<Cliente>();

        using var conexion = ObtenerConexion();
        conexion.Open();

        using var cmd = conexion.CreateCommand();
        cmd.CommandText = "SELECT Id, Nombre, Telefono, Email, Direccion, Ciudad FROM Clientes"; 

        using var reader = cmd.ExecuteReader();
        while (reader.Read())
        {
            clientes.Add(new Cliente
            {
                Id = reader.GetInt32(0),                    
                Nombre = reader.GetString(1),
                Contacto = reader.GetString(2),
                Email = reader.GetString(3),
                Direccion = reader.GetString(4),
                Ciudad = reader.GetString(5)
            });
        }

        return clientes;
    }


    public static void EliminarCliente(string nombre)
    {
        using var conexion = ObtenerConexion();
        conexion.Open();

        using var cmd = conexion.CreateCommand();
        cmd.CommandText = "DELETE FROM Clientes WHERE Nombre = $nombre";
        cmd.Parameters.AddWithValue("$nombre", nombre);

        cmd.ExecuteNonQuery();
    }


    public static void AgregarPedido(Pedido pedido)
    {
        using var conexion = ObtenerConexion();
        conexion.Open();

        using var transaccion = conexion.BeginTransaction();

        try
        {
            // Insertar pedido
            using var cmd = conexion.CreateCommand();
            cmd.Transaction = transaccion;
            cmd.CommandText = """
            INSERT INTO Pedidos (Producto, ClienteId, Cantidad)
            VALUES ($producto, $clienteId, $cantidad);
        """;
            cmd.Parameters.AddWithValue("$producto", pedido.Producto.Nombre);
            cmd.Parameters.AddWithValue("$clienteId", pedido.ClienteId);
            cmd.Parameters.AddWithValue("$cantidad", pedido.Cantidad);
            cmd.ExecuteNonQuery();

            // Actualizar stock
            using var cmdStock = conexion.CreateCommand();
            cmdStock.Transaction = transaccion;
            cmdStock.CommandText = """
            UPDATE Productos
            SET Stock = Stock - $cantidad
            WHERE Nombre = $producto;
        """;
            cmdStock.Parameters.AddWithValue("$producto", pedido.Producto.Nombre);
            cmdStock.Parameters.AddWithValue("$cantidad", pedido.Cantidad);
            cmdStock.ExecuteNonQuery();

            transaccion.Commit();
        }
        catch
        {
            transaccion.Rollback();
            throw;
        }
    }


    public static List<Pedido> ObtenerPedidos()
    {
        var pedidos = new List<Pedido>();
        var productos = ObtenerProductos(); // Usamos productos existentes para obtener el precio

        using var conexion = ObtenerConexion();
        conexion.Open();

        using var cmd = conexion.CreateCommand();
        cmd.CommandText = """
    SELECT p.IdPedido, p.Producto, p.ClienteId, p.Cantidad, c.Nombre
    FROM Pedidos p
    JOIN Clientes c ON p.ClienteId = c.Id;
    """;

        using var reader = cmd.ExecuteReader();
        while (reader.Read())
        {
            var nombreProducto = reader.GetString(1);
            var productoCompleto = productos.FirstOrDefault(p => p.Nombre == nombreProducto);

            pedidos.Add(new Pedido
            {
                IdPedido = reader.GetInt32(0),
                Producto = productoCompleto, // Producto con nombre y precio
                ClienteId = reader.GetInt32(2),
                Cantidad = reader.GetInt32(3),
                Cliente = reader.GetString(4)
            });
        }

        return pedidos;
    }





    public static void EliminarPedido(int id)
    {
        using var conexion = ObtenerConexion();
        conexion.Open();

        using var cmd = conexion.CreateCommand();
        cmd.CommandText = "DELETE FROM Pedidos WHERE IdPedido = $id;";
        cmd.Parameters.AddWithValue("$id", id);

        int filasAfectadas = cmd.ExecuteNonQuery();

        if (filasAfectadas == 0)
        {
            Console.WriteLine($"[!] No se encontró ningún pedido con ID {id} para eliminar.");
        }
    }


    public static List<Pedido> ObtenerPedidosPorCliente(int clienteId)
    {
        var pedidos = new List<Pedido>();

        using var conexion = ObtenerConexion();
        conexion.Open();

        using var cmd = conexion.CreateCommand();
        cmd.CommandText = """
        SELECT p.IdPedido, p.Producto, pr.Precio, p.ClienteId, c.Nombre, p.Cantidad
        FROM Pedidos p
        JOIN Clientes c ON p.ClienteId = c.Id
        JOIN Productos pr ON pr.Nombre = p.Producto
        WHERE p.ClienteId = $clienteId;
    """;

        cmd.Parameters.AddWithValue("$clienteId", clienteId);

        using var reader = cmd.ExecuteReader();
        while (reader.Read())
        {
            var pedido = new Pedido
            {
                IdPedido = reader.GetInt32(0),
                Producto = new Producto
                {
                    Nombre = reader.GetString(1),
                    Precio = reader.GetDouble(2)
                },
                ClienteId = reader.GetInt32(3),
                Cliente = reader.GetString(4),
                Cantidad = reader.GetInt32(5)
            };

            pedidos.Add(pedido);
        }

        return pedidos;
    }


    public static void EliminarFactura(int idFactura)
    {
        using var conexion = ObtenerConexion();
        conexion.Open();

        using var transaccion = conexion.BeginTransaction();
        try
        {
            // Eliminar los detalles primero (por clave foránea)
            using var cmdDetalles = conexion.CreateCommand();
            cmdDetalles.Transaction = transaccion;
            cmdDetalles.CommandText = "DELETE FROM DetalleFactura WHERE IdFactura = $idFactura";
            cmdDetalles.Parameters.AddWithValue("$idFactura", idFactura);
            cmdDetalles.ExecuteNonQuery();

            // Eliminar la factura principal
            using var cmdFactura = conexion.CreateCommand();
            cmdFactura.Transaction = transaccion;
            cmdFactura.CommandText = "DELETE FROM Facturas WHERE Id = $idFactura";
            cmdFactura.Parameters.AddWithValue("$idFactura", idFactura);
            cmdFactura.ExecuteNonQuery();

            transaccion.Commit();
        }
        catch
        {
            transaccion.Rollback();
            throw;
        }
    }

    public static void ActualizarStock(Producto producto)
    {
        using var conexion = ObtenerConexion();
        conexion.Open();

        using var cmd = conexion.CreateCommand();
        cmd.CommandText = "UPDATE Productos SET Stock = $stock WHERE Nombre = $nombre;";
        cmd.Parameters.AddWithValue("$stock", producto.Stock);
        cmd.Parameters.AddWithValue("$nombre", producto.Nombre);
        cmd.ExecuteNonQuery();
    }


    public static List<Factura> ObtenerFacturas()
    {
        var facturas = new List<Factura>();
        using var conexion = ObtenerConexion();
        conexion.Open();

        using var cmd = conexion.CreateCommand();
        cmd.CommandText = "SELECT Id, Cliente, Fecha, Total FROM Facturas";

        using var reader = cmd.ExecuteReader();
        while (reader.Read())
        {
            facturas.Add(new Factura
            {
                Id = reader.GetInt32(0),
                Cliente = reader.GetString(1),
                Fecha = reader.GetDateTime(2),
                Total = reader.GetDouble(3)
            });
        }
        return facturas;
    }

public static void AgregarFacturaConDetalles(Factura factura, List<DetalleFactura> detalles, int clienteId)
{
    using var conexion = ObtenerConexion();
    conexion.Open();

    using var transaccion = conexion.BeginTransaction();
    try
    {
        // Insertar factura
        using var cmdFactura = conexion.CreateCommand();
        cmdFactura.Transaction = transaccion;
        cmdFactura.CommandText = """
        INSERT INTO Facturas (Cliente, Fecha, Total)
        VALUES ($cliente, $fecha, $total);
        SELECT last_insert_rowid();
        """;
        cmdFactura.Parameters.AddWithValue("$cliente", factura.Cliente);
        cmdFactura.Parameters.AddWithValue("$fecha", factura.Fecha.ToString("yyyy-MM-dd HH:mm:ss"));
        cmdFactura.Parameters.AddWithValue("$total", factura.Total);

        var idFactura = Convert.ToInt32(cmdFactura.ExecuteScalar());

        // Insertar detalles
        foreach (var detalle in detalles)
        {
            using var cmdDetalle = conexion.CreateCommand();
            cmdDetalle.Transaction = transaccion;
            cmdDetalle.CommandText = """
            INSERT INTO DetalleFactura (IdFactura, Producto, Cantidad, PrecioUnitario)
            VALUES ($idFactura, $producto, $cantidad, $precio);
            """;
            cmdDetalle.Parameters.AddWithValue("$idFactura", idFactura);
            cmdDetalle.Parameters.AddWithValue("$producto", detalle.Producto);
            cmdDetalle.Parameters.AddWithValue("$cantidad", detalle.Cantidad);
            cmdDetalle.Parameters.AddWithValue("$precio", detalle.PrecioUnitario);
            cmdDetalle.ExecuteNonQuery();
        }

        // Eliminar los pedidos facturados
        using var cmdEliminar = conexion.CreateCommand();
        cmdEliminar.Transaction = transaccion;
        cmdEliminar.CommandText = """
        DELETE FROM Pedidos WHERE ClienteId = $clienteId;
        """;
        cmdEliminar.Parameters.AddWithValue("$clienteId", clienteId);
        cmdEliminar.ExecuteNonQuery();

        transaccion.Commit();
    }
    catch
    {
        transaccion.Rollback();
        throw;
    }
}

    public static List<DetalleFactura> ObtenerDetalleFactura(int idFactura)
    {
        var detalles = new List<DetalleFactura>();

        using var conexion = ObtenerConexion();
        conexion.Open();

        using var cmd = conexion.CreateCommand();
        cmd.CommandText = """
        SELECT Producto, Cantidad, PrecioUnitario
        FROM DetalleFactura
        WHERE IdFactura = $idFactura;
    """;
        cmd.Parameters.AddWithValue("$idFactura", idFactura);

        using var reader = cmd.ExecuteReader();
        while (reader.Read())
        {
            detalles.Add(new DetalleFactura
            {
                Producto = reader.GetString(0),
                Cantidad = reader.GetInt32(1),
                PrecioUnitario = reader.GetDouble(2)
            });
        }

        return detalles;
    }

}
