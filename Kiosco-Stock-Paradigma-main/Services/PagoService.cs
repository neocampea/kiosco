using StockVentas.Models;

namespace StockVentas.Services
{
    // Calcula el precio final de un producto según el medio de pago elegido.
    // Reglas de ejemplo (fácilmente ajustables):
    //  - Efectivo:      10% de descuento
    //  - Transferencia:  5% de descuento
    //  - Debito:        precio normal, sin recargo
    //  - Credito:       15% de recargo
    public class PagoService
    {
        public const decimal DescuentoEfectivo = 0.10m;
        public const decimal DescuentoTransferencia = 0.05m;
        public const decimal RecargoCredito = 0.15m;

        public decimal CalcularPrecioSegunMedioPago(decimal precioBase, MedioPago medioPago)
        {
            return medioPago switch
            {
                MedioPago.Efectivo => precioBase * (1 - DescuentoEfectivo),
                MedioPago.Transferencia => precioBase * (1 - DescuentoTransferencia),
                MedioPago.Debito => precioBase,
                MedioPago.Credito => precioBase * (1 + RecargoCredito),
                _ => precioBase
            };
        }
    }
}
