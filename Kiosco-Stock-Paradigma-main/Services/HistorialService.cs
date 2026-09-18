using System;
using System.Collections.Generic;
using System.IO;

namespace StockVentas.Services
{
    // Registra información histórica de todo lo que pasa en el sistema
    // (altas, bajas, modificaciones, ventas, cancelaciones), tanto en
    // memoria como en un archivo de texto persistente (historial.txt)
    public class HistorialService
    {
        private readonly List<string> _eventos = new List<string>();
        private readonly string _rutaArchivo;

        public HistorialService(string rutaArchivo = "historial.txt")
        {
            _rutaArchivo = rutaArchivo;
        }

        public void RegistrarEvento(string mensaje)
        {
            string linea = $"{DateTime.Now:dd/MM/yyyy HH:mm:ss} - {mensaje}";
            _eventos.Add(linea);

            try
            {
                File.AppendAllText(_rutaArchivo, linea + Environment.NewLine);
            }
            catch (IOException)
            {
                // Si falla la escritura en disco, el evento igual queda en memoria
            }
        }

        public List<string> ConsultarHistorial()
        {
            return _eventos;
        }

        public void MostrarHistorial()
        {
            if (_eventos.Count == 0)
            {
                Console.WriteLine("No hay eventos registrados todavía.");
                return;
            }

            Console.WriteLine("=== HISTORIAL DEL SISTEMA ===");
            foreach (var e in _eventos)
                Console.WriteLine(e);
        }
    }
}
