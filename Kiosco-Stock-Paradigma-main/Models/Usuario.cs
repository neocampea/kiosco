using System;

namespace StockVentas.Models
{
    public class Usuario
    {
        public int Id { get; set; }
        public string NombreUsuario { get; set; } = string.Empty;
        public string NombreCompleto { get; set; } = string.Empty;
        public RolUsuario Rol { get; set; }
        public string PasswordHash { get; set; } = string.Empty;
        public bool Activo { get; set; } = true;
        public DateTime FechaAlta { get; set; } = DateTime.Now;

        public override string ToString()
        {
            return $"[{Id}] {NombreCompleto} ({NombreUsuario}) - {Rol}";
        }
    }
}
