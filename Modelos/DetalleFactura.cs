namespace Modelos;

public struct DetalleFactura
{
	public int IdFactura { get; set; }
	public string Producto { get; set; }
	public int Cantidad { get; set; }
	public double PrecioUnitario { get; set; }
}