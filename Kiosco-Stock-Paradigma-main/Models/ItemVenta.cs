namespace StockVentas.Models
{
    public class ItemVenta
    {
        public int ProductoId { get; set; }
        public string NombreProducto { get; set; } = string.Empty;
        public int Cantidad { get; set; }
        public decimal PrecioUnitario { get; set; } // ya calculado según medio de pago
        public decimal Subtotal => Cantidad * PrecioUnitario;

        public override string ToString()
        {
            return $"  - {NombreProducto} x{Cantidad} @ {PrecioUnitario:C} = {Subtotal:C}";
        }
    }
}
