using System;
using System.Collections.Generic;
using System.Linq;
using Dapper;
using StockVentas.Models;

namespace StockVentas.Services
{
    public class StockService
    {
        private readonly string _connectionString;
        private readonly HistorialService _historial;

        public StockService(string connectionString, HistorialService historial)
        {
            _connectionString = connectionString;
            _historial = historial;
        }

        public Producto RegistrarProducto(string nombre, string descripcion, string categoria, decimal precioBase, int stock)
        {
            using var conn = DbConexion.Crear(_connectionString);

            conn.Execute(
                @"INSERT INTO productos (nombre, descripcion, categoria, precio_base, stock, activo)
                  VALUES (@Nombre, @Descripcion, @Categoria, @PrecioBase, @Stock, 1)",
                new { Nombre = nombre, Descripcion = descripcion, Categoria = categoria, PrecioBase = precioBase, Stock = stock });

            int id = conn.ExecuteScalar<int>("SELECT last_insert_rowid();");

            var producto = new Producto
            {
                Id = id,
                Nombre = nombre,
                Descripcion = descripcion,
                Categoria = categoria,
                PrecioBase = precioBase,
                Stock = stock
            };

            _historial.RegistrarEvento($"Producto registrado: {producto.Nombre} (ID {producto.Id}), stock inicial {stock}");
            return producto;
        }

        public bool ModificarProducto(int id, string? nombre = null, string? descripcion = null,
            string? categoria = null, decimal? precioBase = null, int? stock = null)
        {
            var producto = ObtenerProducto(id);
            if (producto == null) return false;

            if (!string.IsNullOrWhiteSpace(nombre)) producto.Nombre = nombre;
            if (!string.IsNullOrWhiteSpace(descripcion)) producto.Descripcion = descripcion;
            if (!string.IsNullOrWhiteSpace(categoria)) producto.Categoria = categoria;
            if (precioBase.HasValue) producto.PrecioBase = precioBase.Value;
            if (stock.HasValue) producto.Stock = stock.Value;

            using var conn = DbConexion.Crear(_connectionString);
            conn.Execute(
                @"UPDATE productos SET nombre = @Nombre, descripcion = @Descripcion, categoria = @Categoria,
                                        precio_base = @PrecioBase, stock = @Stock
                  WHERE id = @Id",
                new
                {
                    producto.Nombre,
                    producto.Descripcion,
                    producto.Categoria,
                    producto.PrecioBase,
                    producto.Stock,
                    Id = id
                });

            _historial.RegistrarEvento($"Producto modificado: ID {producto.Id} - {producto.Nombre}");
            return true;
        }

        public bool EliminarProducto(int id)
        {
            var producto = ObtenerProducto(id);
            if (producto == null) return false;

            using var conn = DbConexion.Crear(_connectionString);
            conn.Execute("UPDATE productos SET activo = 0 WHERE id = @Id", new { Id = id });

            _historial.RegistrarEvento($"Producto eliminado: ID {producto.Id} - {producto.Nombre}");
            return true;
        }

        public List<Producto> ConsultarDisponibles()
        {
            using var conn = DbConexion.Crear(_connectionString);
            return conn.Query<Producto>(
                "SELECT * FROM productos WHERE activo = 1 AND stock > 0 ORDER BY nombre").ToList();
        }

        public List<Producto> ConsultarTodos()
        {
            using var conn = DbConexion.Crear(_connectionString);
            return conn.Query<Producto>(
                "SELECT * FROM productos WHERE activo = 1 ORDER BY nombre").ToList();
        }

        public List<Producto> BuscarProducto(string criterio)
        {
            criterio = criterio.Trim();

            using var conn = DbConexion.Crear(_connectionString);
            return conn.Query<Producto>(
                @"SELECT * FROM productos
                  WHERE activo = 1
                    AND (CAST(id AS TEXT) = @Criterio
                         OR nombre LIKE @Like
                         OR categoria LIKE @Like)
                  ORDER BY nombre",
                new { Criterio = criterio, Like = $"%{criterio}%" }).ToList();
        }

        public Producto? ObtenerProducto(int id)
        {
            using var conn = DbConexion.Crear(_connectionString);
            return conn.QueryFirstOrDefault<Producto>(
                "SELECT * FROM productos WHERE id = @Id", new { Id = id });
        }

        public bool ActualizarStock(int id, int cantidadADescontar)
        {
            using var conn = DbConexion.Crear(_connectionString);
            int filas = conn.Execute(
                "UPDATE productos SET stock = stock - @Cantidad WHERE id = @Id AND stock >= @Cantidad",
                new { Cantidad = cantidadADescontar, Id = id });
            return filas > 0;
        }

        public void RestaurarStock(int id, int cantidad)
        {
            using var conn = DbConexion.Crear(_connectionString);
            conn.Execute("UPDATE productos SET stock = stock + @Cantidad WHERE id = @Id",
                new { Cantidad = cantidad, Id = id });
        }
    }
}
