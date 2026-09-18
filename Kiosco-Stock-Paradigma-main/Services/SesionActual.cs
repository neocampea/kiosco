using StockVentas.Models;

namespace StockVentas.Services
{
    // Representa quién está logueado ahora, mientras dure el proceso.
    // Sin tokens ni expiración: es una app de escritorio de un solo puesto.
    public class SesionActual
    {
        public Usuario? UsuarioActual { get; private set; }
        public bool HaySesionActiva => UsuarioActual != null;
        public bool EsDueno => UsuarioActual?.Rol == RolUsuario.Dueno;

        public void IniciarSesion(Usuario usuario) => UsuarioActual = usuario;

        public void CerrarSesion() => UsuarioActual = null;
    }
}
