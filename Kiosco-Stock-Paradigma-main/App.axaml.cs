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
                string connectionString;

                try
                {
                    connectionString = CargarConnectionString();
                }
                catch (Exception ex)
                {
                    // Evita que la aplicación se cierre sin explicación si falta la configuración.
                    connectionString = string.Empty;
                    Console.Error.WriteLine(ex.Message);
                }

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
            // Permite configurar la conexión sin depender de rutas específicas de Windows,
            // macOS o Linux. La variable de entorno tiene prioridad.
            string? variableEntorno = Environment.GetEnvironmentVariable("STOCKVENTAS_CONNECTION_STRING");
            if (!string.IsNullOrWhiteSpace(variableEntorno))
                return variableEntorno.Trim();

            string[] rutas = {
                Path.Combine(AppContext.BaseDirectory, "db.config"),
                Path.Combine(Directory.GetCurrentDirectory(), "db.config")
            };

            foreach (string ruta in rutas)
            {
                if (File.Exists(ruta))
                    return File.ReadAllText(ruta).Trim();
            }

            throw new FileNotFoundException(
                "No se encontró la configuración de la base de datos. " +
                "Copiá 'db.config.example' como 'db.config' y completá los datos, " +
                "o configurá la variable de entorno STOCKVENTAS_CONNECTION_STRING.");
        }
    }
}
