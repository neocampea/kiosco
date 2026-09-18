using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using StockVentas.Services;
using StockVentas.Views;

namespace StockVentas
{
    public partial class App : Application
    {
        public override void Initialize()
        {
            AvaloniaXamlLoader.Load(this);
        }

        public override void OnFrameworkInitializationCompleted()
        {
            if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            {
                DbConexion.InicializarBaseDatos();
                string connectionString = DbConexion.ObtenerConnectionString();

                var historial = new HistorialService();
                var stockService = new StockService(connectionString, historial);
                var pagoService = new PagoService();
                var ventaService = new VentaService(connectionString, stockService, pagoService, historial);
                var authService = new AuthService(connectionString);
                var empleadoService = new EmpleadoService(connectionString, authService, historial);
                var sesion = new SesionActual();

                desktop.MainWindow = new LoginWindow(
                    stockService, pagoService, ventaService, historial, authService, empleadoService, sesion);
            }

            base.OnFrameworkInitializationCompleted();
        }
    }
}
