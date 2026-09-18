using System;
using Avalonia;

namespace StockVentas
{
    // Punto de entrada de la aplicación de escritorio Avalonia.
    // Reemplaza al Program.cs de consola (ver Legacy/ProgramConsola.cs.txt).
    internal static class Program
    {
        [STAThread]
        public static int Main(string[] args)
            => BuildAvaloniaApp().StartWithClassicDesktopLifetime(args);

        public static AppBuilder BuildAvaloniaApp()
            => AppBuilder.Configure<App>()
                .UsePlatformDetect()
                .WithInterFont()
                .LogToTrace();
    }
}
