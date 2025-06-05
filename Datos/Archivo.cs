using Modelos;
using System.IO;

namespace Datos;

public static class Archivos
{
    public static List<Producto> Productos = new();
    public static List<Pedido> Pedidos = new();
    public static List<Proveedor> Proveedores = new();

    const string archivoProductos = "productos.txt";
    const string archivoPedidos = "pedidos.txt";
    const string archivoProveedores = "proveedores.txt";

    public static void CargarTodo()
    {
        if (File.Exists(archivoProductos))
            Productos.AddRange(File.ReadAllLines(archivoProductos).Select(Producto.FromString));
        if (File.Exists(archivoProveedores))
            Proveedores.AddRange(File.ReadAllLines(archivoProveedores).Select(Proveedor.FromString));
    }

    public static void GuardarTodo()
    {
        File.WriteAllLines(archivoProductos, Productos.Select(p => p.ToString()));
        File.WriteAllLines(archivoPedidos, Pedidos.Select(p => p.ToString()));
        File.WriteAllLines(archivoProveedores, Proveedores.Select(p => p.ToString()));
    }
}
