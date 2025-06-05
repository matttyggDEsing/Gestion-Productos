public struct Factura
{
    public int Id;
    public string Cliente;
    public int ClienteId; // <- NECESARIO para eliminar los pedidos luego
    public DateTime Fecha;
    public double Total;
}
