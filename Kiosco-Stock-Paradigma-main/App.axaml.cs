using System;
using System.IO;
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
                string connectionString = CargarConnectionString();

                // Los servicios se arman una única vez, igual que hacía Program.cs en la versión de consola.
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

        private static string CargarConnectionString()
        {
            const string ruta = "db.config";
            if (!File.Exists(ruta))
            {
                throw new FileNotFoundException(
                    $"No se encontró '{ruta}'. Copiá 'db.config.example' a 'db.config' y completá la contraseña de la base de datos.");
            }

            return File.ReadAllText(ruta).Trim();
        }
    }
}
