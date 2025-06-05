namespace Modelos;

public struct Pedido
{
    public int IdPedido { get; set; }
    public Producto Producto { get; set; }
    public int ClienteId { get; set; }
    public string Cliente { get; set; }
    public int Cantidad { get; set; }
}