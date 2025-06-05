namespace Modelos;
public struct Producto



{
    public string Nombre;
    public string Descripcion;
    public string Proveedor;
    public double Precio;
    public int Stock;

    public override string ToString() => $"{Nombre}|{Descripcion}|{Proveedor}|{Precio}|{Stock}";

    public static Producto FromString(string data)
    {
        var parts = data.Split('|');
        return new Producto
        {
            Nombre = parts[0],
            Descripcion = parts[1],
            Proveedor = parts[2],
            Precio = double.Parse(parts[3]),
            Stock = int.Parse(parts[4])
        };
    }
}
