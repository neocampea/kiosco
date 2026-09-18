using System;
using System.Security.Cryptography;
using Dapper;
using StockVentas.Models;

namespace StockVentas.Services
{
    // Autenticación de usuarios: hashing de contraseñas (PBKDF2) e inicio de sesión.
    public class AuthService
    {
        private const int Iteraciones = 100_000;
        private const int TamanioSalt = 16;
        private const int TamanioHash = 32;

        private readonly string _connectionString;

        public AuthService(string connectionString)
        {
            _connectionString = connectionString;
        }

        // Si todavía no existe ningún usuario con rol Dueño, crea uno por defecto
        // (admin/admin123) para poder ingresar la primera vez.
        public bool AsegurarUsuarioDuenoBootstrap()
        {
            using var conn = DbConexion.Crear(_connectionString);

            int cantidadDuenos = conn.ExecuteScalar<int>(
                "SELECT COUNT(*) FROM usuarios WHERE rol = 'Dueno'");

            if (cantidadDuenos > 0) return false;

            string hash = HashearPassword("admin123");
            conn.Execute(
                @"INSERT INTO usuarios (nombre_usuario, nombre_completo, rol, password_hash, activo)
                  VALUES (@NombreUsuario, @NombreCompleto, @Rol, @PasswordHash, 1)",
                new
                {
                    NombreUsuario = "admin",
                    NombreCompleto = "Administrador",
                    Rol = RolUsuario.Dueno.ToString(),
                    PasswordHash = hash
                });

            return true;
        }

        public (bool Ok, string Mensaje, Usuario? Usuario) IniciarSesion(string nombreUsuario, string password)
        {
            if (string.IsNullOrWhiteSpace(nombreUsuario) || string.IsNullOrWhiteSpace(password))
                return (false, "Usuario y contraseña son obligatorios.", null);

            using var conn = DbConexion.Crear(_connectionString);

            var fila = conn.QueryFirstOrDefault<UsuarioFila>(
                @"SELECT id, nombre_usuario, nombre_completo, rol, password_hash, activo, fecha_alta
                  FROM usuarios WHERE nombre_usuario = @NombreUsuario",
                new { NombreUsuario = nombreUsuario });

            if (fila == null)
                return (false, "Usuario o contraseña incorrectos.", null);

            if (!fila.activo)
                return (false, "El usuario está desactivado.", null);

            if (!VerificarPassword(password, fila.password_hash))
                return (false, "Usuario o contraseña incorrectos.", null);

            var usuario = new Usuario
            {
                Id = fila.id,
                NombreUsuario = fila.nombre_usuario,
                NombreCompleto = fila.nombre_completo,
                Rol = Enum.Parse<RolUsuario>(fila.rol),
                PasswordHash = fila.password_hash,
                Activo = fila.activo,
                FechaAlta = fila.fecha_alta
            };

            return (true, "Inicio de sesión correcto.", usuario);
        }

        public (bool Ok, string Mensaje) CambiarPassword(int usuarioId, string nuevaPassword)
        {
            if (string.IsNullOrWhiteSpace(nuevaPassword) || nuevaPassword.Length < 4)
                return (false, "La nueva contraseña debe tener al menos 4 caracteres.");

            using var conn = DbConexion.Crear(_connectionString);
            int filas = conn.Execute(
                "UPDATE usuarios SET password_hash = @Hash WHERE id = @Id",
                new { Hash = HashearPassword(nuevaPassword), Id = usuarioId });

            return filas > 0
                ? (true, "Contraseña actualizada correctamente.")
                : (false, "No se encontró el usuario.");
        }

        public string HashearPassword(string password)
        {
            byte[] salt = RandomNumberGenerator.GetBytes(TamanioSalt);
            byte[] hash = Rfc2898DeriveBytes.Pbkdf2(password, salt, Iteraciones, HashAlgorithmName.SHA256, TamanioHash);
            return $"{Iteraciones}.{Convert.ToBase64String(salt)}.{Convert.ToBase64String(hash)}";
        }

        public bool VerificarPassword(string password, string hashAlmacenado)
        {
            string[] partes = hashAlmacenado.Split('.');
            if (partes.Length != 3) return false;

            int iteraciones = int.Parse(partes[0]);
            byte[] salt = Convert.FromBase64String(partes[1]);
            byte[] hashEsperado = Convert.FromBase64String(partes[2]);

            byte[] hashCalculado = Rfc2898DeriveBytes.Pbkdf2(
                password, salt, iteraciones, HashAlgorithmName.SHA256, hashEsperado.Length);

            return CryptographicOperations.FixedTimeEquals(hashCalculado, hashEsperado);
        }

        // Forma tal cual vienen las columnas de la tabla `usuarios` (snake_case) desde Dapper.
        private sealed class UsuarioFila
        {
            public int id { get; set; }
            public string nombre_usuario { get; set; } = string.Empty;
            public string nombre_completo { get; set; } = string.Empty;
            public string rol { get; set; } = string.Empty;
            public string password_hash { get; set; } = string.Empty;
            public bool activo { get; set; }
            public DateTime fecha_alta { get; set; }
        }
    }
}
