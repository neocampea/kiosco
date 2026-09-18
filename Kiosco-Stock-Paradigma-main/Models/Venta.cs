using System;
using System.Collections.Generic;
using System.Linq;

namespace StockVentas.Models
{
    public enum EstadoVenta
    {
        Activa,
        Cancelada
    }

    public class Venta
    {
        public int Id { get; set; }
        public DateTime Fecha { get; set; } = DateTime.Now;
        public List<ItemVenta> Items { get; set; } = new List<ItemVenta>();
        public MedioPago MedioPago { get; set; }
        public EstadoVenta Estado { get; set; } = EstadoVenta.Activa;
        public int UsuarioId { get; set; }
        public string NombreUsuario { get; set; } = string.Empty;
        public decimal Total => Items.Sum(i => i.Subtotal);

        public override string ToString()
        {
            var detalle = string.Join("\n", Items.Select(i => i.ToString()));
            return $"Venta #{Id} | {Fecha:dd/MM/yyyy HH:mm} | Medio de pago: {MedioPago} | Estado: {Estado} | Empleado: {NombreUsuario}\n{detalle}\n  TOTAL: {Total:C}";
        }
    }
}
