using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Media;
using StockVentas.Services;

namespace StockVentas.Views
{
    public partial class LoginWindow : Window
    {
        private readonly StockService _stockService;
        private readonly PagoService _pagoService;
        private readonly VentaService _ventaService;
        private readonly HistorialService _historial;
        private readonly AuthService _authService;
        private readonly EmpleadoService _empleadoService;
        private readonly SesionActual _sesion;

        // Constructor sin parámetros para el Previewer de Avalonia (no se usa en ejecución real).
        public LoginWindow() : this(
            new StockService("", new HistorialService()), new PagoService(),
            new VentaService("", new StockService("", new HistorialService()), new PagoService(), new HistorialService()),
            new HistorialService(), new AuthService(""), new EmpleadoService("", new AuthService(""), new HistorialService()),
            new SesionActual())
        { }

        public LoginWindow(StockService stockService, PagoService pagoService, VentaService ventaService,
                            HistorialService historial, AuthService authService, EmpleadoService empleadoService,
                            SesionActual sesion)
        {
            InitializeComponent();

            _stockService = stockService;
            _pagoService = pagoService;
            _ventaService = ventaService;
            _historial = historial;
            _authService = authService;
            _empleadoService = empleadoService;
            _sesion = sesion;

            try
            {
                bool seCreoBootstrap = _authService.AsegurarUsuarioDuenoBootstrap();
                if (seCreoBootstrap)
                {
                    MostrarEstado(
                        "Se creó el usuario Dueño por defecto: admin / admin123. Cambiá la contraseña luego de ingresar.",
                        esError: false);
                }
            }
            catch (System.Exception ex)
            {
                MostrarEstado($"No se pudo conectar a la base de datos: {ex.Message}", esError: true);
                btnIngresar.IsEnabled = false;
            }
        }

        private void MostrarEstado(string mensaje, bool esError)
        {
            txtEstadoLogin.Text = mensaje;
            txtEstadoLogin.Foreground = esError ? Brushes.Crimson : Brushes.SeaGreen;
        }

        private void TxtPassword_KeyDown(object? sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
                BtnIngresar_Click(sender, new RoutedEventArgs());
        }

        private void BtnIngresar_Click(object? sender, RoutedEventArgs e)
        {
            var (ok, mensaje, usuario) = _authService.IniciarSesion(txtUsuario.Text ?? "", txtPassword.Text ?? "");
            if (!ok || usuario == null)
            {
                MostrarEstado(mensaje, esError: true);
                return;
            }

            _sesion.IniciarSesion(usuario);
            _historial.RegistrarEvento($"Inicio de sesión: {usuario.NombreUsuario} ({usuario.Rol})");

            var main = new MainWindow(_stockService, _pagoService, _ventaService, _historial, _empleadoService, _authService, _sesion);
            main.Show();
            Close();
        }
    }
}
