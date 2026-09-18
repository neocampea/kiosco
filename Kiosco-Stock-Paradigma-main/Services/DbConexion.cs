using System;
using System.IO;
using Microsoft.Data.Sqlite;

namespace StockVentas.Services
{
    // Conexión local a SQLite. La base de datos vive en el directorio de
    // datos de la aplicación y no necesita un servidor MySQL.
    public static class DbConexion
    {
        private static readonly object Sync = new object();
        private static bool _inicializada;

        public static string ObtenerRutaBaseDatos()
        {
            string baseDir = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            if (string.IsNullOrWhiteSpace(baseDir))
                baseDir = AppContext.BaseDirectory;

            string directorio = Path.Combine(baseDir, "StockVentas");
            Directory.CreateDirectory(directorio);
            return Path.Combine(directorio, "stockventas.db");
        }

        public static string ObtenerConnectionString()
        {
            return new SqliteConnectionStringBuilder
            {
                DataSource = ObtenerRutaBaseDatos(),
                ForeignKeys = true,
                Mode = SqliteOpenMode.ReadWriteCreate,
                Cache = SqliteCacheMode.Shared
            }.ToString();
        }

        public static SqliteConnection Crear(string? connectionString = null)
        {
            var conn = new SqliteConnection(connectionString ?? ObtenerConnectionString());
            conn.Open();
            return conn;
        }

        public static void InicializarBaseDatos()
        {
            lock (Sync)
            {
                if (_inicializada)
                    return;

                using var conn = Crear();
                using var command = conn.CreateCommand();
                command.CommandText = @"
CREATE TABLE IF NOT EXISTS usuarios (
    id INTEGER PRIMARY KEY AUTOINCREMENT,
    nombre_usuario TEXT NOT NULL UNIQUE,
    nombre_completo TEXT NOT NULL,
    rol TEXT NOT NULL CHECK (rol IN ('Dueno','Empleado')),
    password_hash TEXT NOT NULL,
    activo INTEGER NOT NULL DEFAULT 1,
    fecha_alta TEXT NOT NULL DEFAULT CURRENT_TIMESTAMP
);

CREATE TABLE IF NOT EXISTS productos (
    id INTEGER PRIMARY KEY AUTOINCREMENT,
    nombre TEXT NOT NULL,
    descripcion TEXT NOT NULL DEFAULT '',
    categoria TEXT NOT NULL DEFAULT '',
    precio_base NUMERIC NOT NULL,
    stock INTEGER NOT NULL DEFAULT 0,
    activo INTEGER NOT NULL DEFAULT 1,
    fecha_alta TEXT NOT NULL DEFAULT CURRENT_TIMESTAMP
);

CREATE TABLE IF NOT EXISTS ventas (
    id INTEGER PRIMARY KEY AUTOINCREMENT,
    fecha TEXT NOT NULL DEFAULT CURRENT_TIMESTAMP,
    medio_pago TEXT NOT NULL CHECK (medio_pago IN ('Efectivo','Transferencia','Debito','Credito')),
    estado TEXT NOT NULL DEFAULT 'Activa' CHECK (estado IN ('Activa','Cancelada')),
    usuario_id INTEGER NOT NULL,
    FOREIGN KEY (usuario_id) REFERENCES usuarios(id)
);

CREATE TABLE IF NOT EXISTS items_venta (
    id INTEGER PRIMARY KEY AUTOINCREMENT,
    venta_id INTEGER NOT NULL,
    producto_id INTEGER NOT NULL,
    nombre_producto TEXT NOT NULL,
    cantidad INTEGER NOT NULL,
    precio_unitario NUMERIC NOT NULL,
    FOREIGN KEY (venta_id) REFERENCES ventas(id) ON DELETE CASCADE,
    FOREIGN KEY (producto_id) REFERENCES productos(id)
);

CREATE INDEX IF NOT EXISTS idx_ventas_usuario_fecha ON ventas(usuario_id, fecha);
";
                command.ExecuteNonQuery();
                _inicializada = true;
            }
        }
    }
}
