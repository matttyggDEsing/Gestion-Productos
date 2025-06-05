namespace Modelos;
public struct Proveedor
{
    public string Nombre;
    public string Contacto;
    public string Email;

    public override string ToString() => $"{Nombre}|{Contacto}|{Email}";

    public static Proveedor FromString(string data)
    {
        var parts = data.Split('|');
        return new Proveedor { Nombre = parts[0], Contacto = parts[1], Email = parts[2] };
    }
}
