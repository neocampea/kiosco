using System;
using System.Collections.Generic;
using System.Linq;
using Dapper;
using StockVentas.Models;

namespace StockVentas.Services
{
    // Gestión de usuarios (empleados y dueños). Solo un usuario con rol Dueño
    // puede ejecutar estas operaciones; cada método valida el rol de quien
    // lo ejecuta además de que la UI oculte estos controles a los empleados.
    public class EmpleadoService
    {
        private readonly string _connectionString;
        private readonly AuthService _authService;
        private readonly HistorialService _historial;

        public EmpleadoService(string connectionString, AuthService authService, HistorialService historial)
        {
            _connectionString = connectionString;
            _authService = authService;
            _historial = historial;
        }

        public List<Usuario> ConsultarEmpleados(bool incluirInactivos = false)
        {
            using var conn = DbConexion.Crear(_connectionString);

            string sql = @"SELECT id, nombre_usuario AS NombreUsuario, nombre_completo AS NombreCompleto,
                                   rol AS Rol, password_hash AS PasswordHash, activo AS Activo, fecha_alta AS FechaAlta
                            FROM usuarios";
            if (!incluirInactivos) sql += " WHERE activo = 1";
            sql += " ORDER BY nombre_completo";

            return conn.Query<UsuarioFila>(sql)
                .Select(MapearUsuario)
                .ToList();
        }

        public (bool Ok, string Mensaje, Usuario? Usuario) RegistrarEmpleado(
            Usuario ejecutadoPor, string nombreUsuario, string nombreCompleto, string passwordInicial, RolUsuario rol)
        {
            if (ejecutadoPor.Rol != RolUsuario.Dueno)
                return (false, "Solo el Dueño puede gestionar empleados.", null);

            if (string.IsNullOrWhiteSpace(nombreUsuario) || string.IsNullOrWhiteSpace(nombreCompleto))
                return (false, "Usuario y nombre completo son obligatorios.", null);
            if (string.IsNullOrWhiteSpace(passwordInicial) || passwordInicial.Length < 4)
                return (false, "La contraseña debe tener al menos 4 caracteres.", null);

            using var conn = DbConexion.Crear(_connectionString);

            int existe = conn.ExecuteScalar<int>(
                "SELECT COUNT(*) FROM usuarios WHERE nombre_usuario = @NombreUsuario",
                new { NombreUsuario = nombreUsuario });
            if (existe > 0)
                return (false, "Ya existe un usuario con ese nombre.", null);

            string hash = _authService.HashearPassword(passwordInicial);

            int id = conn.ExecuteScalar<int>(
                @"INSERT INTO usuarios (nombre_usuario, nombre_completo, rol, password_hash, activo)
                  VALUES (@NombreUsuario, @NombreCompleto, @Rol, @PasswordHash, 1);
                  SELECT LAST_INSERT_ID();",
                new
                {
                    NombreUsuario = nombreUsuario,
                    NombreCompleto = nombreCompleto,
                    Rol = rol.ToString(),
                    PasswordHash = hash
                });

            var usuario = new Usuario
            {
                Id = id,
                NombreUsuario = nombreUsuario,
                NombreCompleto = nombreCompleto,
                Rol = rol,
                Activo = true
            };

            _historial.RegistrarEvento($"Empleado registrado: {nombreCompleto} ({nombreUsuario}), rol {rol}, por {ejecutadoPor.NombreUsuario}");
            return (true, "Empleado registrado correctamente.", usuario);
        }

        public (bool Ok, string Mensaje) ModificarEmpleado(Usuario ejecutadoPor, int id, string nombreCompleto, RolUsuario rol)
        {
            if (ejecutadoPor.Rol != RolUsuario.Dueno)
                return (false, "Solo el Dueño puede gestionar empleados.");
            if (string.IsNullOrWhiteSpace(nombreCompleto))
                return (false, "El nombre completo es obligatorio.");

            using var conn = DbConexion.Crear(_connectionString);
            int filas = conn.Execute(
                "UPDATE usuarios SET nombre_completo = @NombreCompleto, rol = @Rol WHERE id = @Id",
                new { NombreCompleto = nombreCompleto, Rol = rol.ToString(), Id = id });

            if (filas == 0) return (false, "No se encontró el empleado.");

            _historial.RegistrarEvento($"Empleado modificado: ID {id} - {nombreCompleto} ({rol}), por {ejecutadoPor.NombreUsuario}");
            return (true, "Empleado modificado correctamente.");
        }

        public (bool Ok, string Mensaje) DesactivarEmpleado(Usuario ejecutadoPor, int id)
            => CambiarActivo(ejecutadoPor, id, activo: false);

        public (bool Ok, string Mensaje) ReactivarEmpleado(Usuario ejecutadoPor, int id)
            => CambiarActivo(ejecutadoPor, id, activo: true);

        private (bool Ok, string Mensaje) CambiarActivo(Usuario ejecutadoPor, int id, bool activo)
        {
            if (ejecutadoPor.Rol != RolUsuario.Dueno)
                return (false, "Solo el Dueño puede gestionar empleados.");
            if (ejecutadoPor.Id == id)
                return (false, "No podés desactivar tu propio usuario.");

            using var conn = DbConexion.Crear(_connectionString);
            int filas = conn.Execute("UPDATE usuarios SET activo = @Activo WHERE id = @Id",
                new { Activo = activo, Id = id });

            if (filas == 0) return (false, "No se encontró el empleado.");

            _historial.RegistrarEvento($"Empleado {(activo ? "reactivado" : "desactivado")}: ID {id}, por {ejecutadoPor.NombreUsuario}");
            return (true, activo ? "Empleado reactivado." : "Empleado desactivado.");
        }

        public (bool Ok, string Mensaje) ResetearPassword(Usuario ejecutadoPor, int id, string nuevaPassword)
        {
            if (ejecutadoPor.Rol != RolUsuario.Dueno)
                return (false, "Solo el Dueño puede gestionar empleados.");

            var (ok, mensaje) = _authService.CambiarPassword(id, nuevaPassword);
            if (ok)
                _historial.RegistrarEvento($"Contraseña reseteada para el usuario ID {id}, por {ejecutadoPor.NombreUsuario}");
            return (ok, mensaje);
        }

        private static Usuario MapearUsuario(UsuarioFila f) => new Usuario
        {
            Id = f.id,
            NombreUsuario = f.NombreUsuario,
            NombreCompleto = f.NombreCompleto,
            Rol = Enum.Parse<RolUsuario>(f.Rol),
            PasswordHash = f.PasswordHash,
            Activo = f.Activo,
            FechaAlta = f.FechaAlta
        };

        private sealed class UsuarioFila
        {
            public int id { get; set; }
            public string NombreUsuario { get; set; } = string.Empty;
            public string NombreCompleto { get; set; } = string.Empty;
            public string Rol { get; set; } = string.Empty;
            public string PasswordHash { get; set; } = string.Empty;
            public bool Activo { get; set; }
            public DateTime FechaAlta { get; set; }
        }
    }
}
