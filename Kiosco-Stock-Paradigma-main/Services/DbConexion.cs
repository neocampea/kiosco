using MySqlConnector;

namespace StockVentas.Services
{
    // Helper mínimo para abrir conexiones a MySQL/MariaDB. Cada método de
    // los services abre y cierra su propia conexión (using), apoyándose
    // en el pooling que ya provee MySqlConnector.
    public static class DbConexion
    {
        public static MySqlConnection Crear(string connectionString)
        {
            var conn = new MySqlConnection(connectionString);
            conn.Open();
            return conn;
        }
    }
}
