using System;

namespace StockVentas.Models
{
    public class Producto
    {
        public int Id { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public string Descripcion { get; set; } = string.Empty;
        public string Categoria { get; set; } = string.Empty;
        public decimal PrecioBase { get; set; }
        public int Stock { get; set; }
        public bool Activo { get; set; } = true;
        public DateTime FechaAlta { get; set; } = DateTime.Now;

        public override string ToString()
        {
            return $"[{Id}] {Nombre} | Categoria: {Categoria} | Precio base: {PrecioBase:C} | Stock: {Stock}";
        }
    }
}
