namespace StockVentas.Models
{
    // DTO de reporte: totales de ventas activas agrupados por empleado.
    public class ResumenVentaEmpleado
    {
        public int UsuarioId { get; set; }
        public string NombreUsuario { get; set; } = string.Empty;
        public int CantidadVentas { get; set; }
        public decimal TotalVendido { get; set; }
    }
}
