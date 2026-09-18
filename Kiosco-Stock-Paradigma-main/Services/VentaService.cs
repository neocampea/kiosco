using System;
using System.Collections.Generic;
using System.Linq;
using Dapper;
using StockVentas.Models;

namespace StockVentas.Services
{
    public class VentaService
    {
        private readonly string _connectionString;
        private readonly StockService _stockService;
        private readonly PagoService _pagoService;
        private readonly HistorialService _historial;

        public VentaService(string connectionString, StockService stockService, PagoService pagoService, HistorialService historial)
        {
            _connectionString = connectionString;
            _stockService = stockService;
            _pagoService = pagoService;
            _historial = historial;
        }

        // Registrar venta: recibe una lista de (productoId, cantidad), el medio de pago
        // y el usuario de la sesión que la está registrando.
        public (bool Ok, string Mensaje, Venta? Venta) RegistrarVenta(
            List<(int ProductoId, int Cantidad)> pedido, MedioPago medioPago, int usuarioId)
        {
            var items = new List<ItemVenta>();

            // Validar stock antes de confirmar nada
            foreach (var (productoId, cantidad) in pedido)
            {
                var producto = _stockService.ObtenerProducto(productoId);
                if (producto == null)
                    return (false, $"Producto ID {productoId} no existe.", null);
                if (producto.Stock < cantidad)
                    return (false, $"Stock insuficiente para '{producto.Nombre}' (disponible: {producto.Stock}).", null);

                decimal precioFinal = _pagoService.CalcularPrecioSegunMedioPago(producto.PrecioBase, medioPago);
                items.Add(new ItemVenta
                {
                    ProductoId = producto.Id,
                    NombreProducto = producto.Nombre,
                    Cantidad = cantidad,
                    PrecioUnitario = precioFinal
                });
            }

            using var conn = DbConexion.Crear(_connectionString);
            using var tx = conn.BeginTransaction();
            try
            {
                foreach (var (productoId, cantidad) in pedido)
                {
                    int filas = conn.Execute(
                        "UPDATE productos SET stock = stock - @Cantidad WHERE id = @Id AND stock >= @Cantidad",
                        new { Cantidad = cantidad, Id = productoId }, tx);
                    if (filas == 0)
                        throw new InvalidOperationException($"Stock insuficiente para el producto ID {productoId}.");
                }

                int ventaId = conn.ExecuteScalar<int>(
                    @"INSERT INTO ventas (medio_pago, estado, usuario_id)
                      VALUES (@MedioPago, 'Activa', @UsuarioId);
                      SELECT LAST_INSERT_ID();",
                    new { MedioPago = medioPago.ToString(), UsuarioId = usuarioId }, tx);

                foreach (var item in items)
                {
                    conn.Execute(
                        @"INSERT INTO items_venta (venta_id, producto_id, nombre_producto, cantidad, precio_unitario)
                          VALUES (@VentaId, @ProductoId, @NombreProducto, @Cantidad, @PrecioUnitario)",
                        new
                        {
                            VentaId = ventaId,
                            item.ProductoId,
                            item.NombreProducto,
                            item.Cantidad,
                            item.PrecioUnitario
                        }, tx);
                }

                tx.Commit();

                var venta = ObtenerVenta(ventaId)!;
                _historial.RegistrarEvento(
                    $"Venta registrada: #{venta.Id}, medio de pago {medioPago}, total {venta.Total:C}, empleado {venta.NombreUsuario}");
                return (true, "Venta registrada correctamente.", venta);
            }
            catch (Exception ex)
            {
                tx.Rollback();
                return (false, $"No se pudo registrar la venta: {ex.Message}", null);
            }
        }

        // Cancelar venta: restaura el stock y marca la venta como cancelada
        public (bool Ok, string Mensaje) CancelarVenta(int ventaId)
        {
            var venta = ObtenerVenta(ventaId);
            if (venta == null)
                return (false, "La venta no existe.");
            if (venta.Estado == EstadoVenta.Cancelada)
                return (false, "La venta ya estaba cancelada.");

            using var conn = DbConexion.Crear(_connectionString);
            using var tx = conn.BeginTransaction();
            try
            {
                foreach (var item in venta.Items)
                {
                    conn.Execute("UPDATE productos SET stock = stock + @Cantidad WHERE id = @Id",
                        new { Cantidad = item.Cantidad, Id = item.ProductoId }, tx);
                }

                conn.Execute("UPDATE ventas SET estado = 'Cancelada' WHERE id = @Id", new { Id = ventaId }, tx);

                tx.Commit();

                _historial.RegistrarEvento($"Venta cancelada: #{venta.Id}, stock restaurado");
                return (true, "Venta cancelada y stock restaurado.");
            }
            catch (Exception ex)
            {
                tx.Rollback();
                return (false, $"No se pudo cancelar la venta: {ex.Message}");
            }
        }

        // Consultar ventas, con filtros opcionales por empleado y rango de fechas.
        public List<Venta> ConsultarVentas(int? usuarioId = null, DateTime? desde = null, DateTime? hasta = null)
        {
            using var conn = DbConexion.Crear(_connectionString);

            string sql = @"SELECT v.id, v.fecha, v.medio_pago, v.estado, v.usuario_id, u.nombre_completo
                            FROM ventas v
                            JOIN usuarios u ON u.id = v.usuario_id
                            WHERE 1=1";
            var parametros = new DynamicParameters();

            if (usuarioId.HasValue)
            {
                sql += " AND v.usuario_id = @UsuarioId";
                parametros.Add("UsuarioId", usuarioId.Value);
            }
            if (desde.HasValue)
            {
                sql += " AND v.fecha >= @Desde";
                parametros.Add("Desde", desde.Value.Date);
            }
            if (hasta.HasValue)
            {
                sql += " AND v.fecha < @Hasta";
                parametros.Add("Hasta", hasta.Value.Date.AddDays(1));
            }
            sql += " ORDER BY v.fecha DESC";

            var filas = conn.Query<VentaFila>(sql, parametros).ToList();
            if (filas.Count == 0) return new List<Venta>();

            var ids = filas.Select(f => f.id).ToList();
            var items = conn.Query<ItemVentaFila>(
                "SELECT venta_id, producto_id, nombre_producto, cantidad, precio_unitario FROM items_venta WHERE venta_id IN @Ids",
                new { Ids = ids }).ToList();

            return filas.Select(f => MapearVenta(f, items.Where(i => i.venta_id == f.id))).ToList();
        }

        public Venta? ObtenerVenta(int id)
        {
            using var conn = DbConexion.Crear(_connectionString);

            var fila = conn.QueryFirstOrDefault<VentaFila>(
                @"SELECT v.id, v.fecha, v.medio_pago, v.estado, v.usuario_id, u.nombre_completo
                  FROM ventas v JOIN usuarios u ON u.id = v.usuario_id
                  WHERE v.id = @Id", new { Id = id });
            if (fila == null) return null;

            var items = conn.Query<ItemVentaFila>(
                "SELECT venta_id, producto_id, nombre_producto, cantidad, precio_unitario FROM items_venta WHERE venta_id = @Id",
                new { Id = id }).ToList();

            return MapearVenta(fila, items);
        }

        // Totales de ventas activas agrupados por empleado, para el reporte de "caja por empleado".
        public List<ResumenVentaEmpleado> ConsultarResumenPorEmpleado(DateTime? desde = null, DateTime? hasta = null)
        {
            using var conn = DbConexion.Crear(_connectionString);

            string sql = @"SELECT v.usuario_id AS UsuarioId, u.nombre_completo AS NombreUsuario,
                                   COUNT(DISTINCT v.id) AS CantidadVentas,
                                   COALESCE(SUM(i.cantidad * i.precio_unitario), 0) AS TotalVendido
                            FROM ventas v
                            JOIN usuarios u ON u.id = v.usuario_id
                            JOIN items_venta i ON i.venta_id = v.id
                            WHERE v.estado = 'Activa'";
            var parametros = new DynamicParameters();

            if (desde.HasValue)
            {
                sql += " AND v.fecha >= @Desde";
                parametros.Add("Desde", desde.Value.Date);
            }
            if (hasta.HasValue)
            {
                sql += " AND v.fecha < @Hasta";
                parametros.Add("Hasta", hasta.Value.Date.AddDays(1));
            }

            sql += " GROUP BY v.usuario_id, u.nombre_completo ORDER BY TotalVendido DESC";

            return conn.Query<ResumenVentaEmpleado>(sql, parametros).ToList();
        }

        private static Venta MapearVenta(VentaFila f, IEnumerable<ItemVentaFila> items) => new Venta
        {
            Id = f.id,
            Fecha = f.fecha,
            MedioPago = Enum.Parse<MedioPago>(f.medio_pago),
            Estado = Enum.Parse<EstadoVenta>(f.estado),
            UsuarioId = f.usuario_id,
            NombreUsuario = f.nombre_completo,
            Items = items.Select(i => new ItemVenta
            {
                ProductoId = i.producto_id,
                NombreProducto = i.nombre_producto,
                Cantidad = i.cantidad,
                PrecioUnitario = i.precio_unitario
            }).ToList()
        };

        private sealed class VentaFila
        {
            public int id { get; set; }
            public DateTime fecha { get; set; }
            public string medio_pago { get; set; } = string.Empty;
            public string estado { get; set; } = string.Empty;
            public int usuario_id { get; set; }
            public string nombre_completo { get; set; } = string.Empty;
        }

        private sealed class ItemVentaFila
        {
            public int venta_id { get; set; }
            public int producto_id { get; set; }
            public string nombre_producto { get; set; } = string.Empty;
            public int cantidad { get; set; }
            public decimal precio_unitario { get; set; }
        }
    }
}
