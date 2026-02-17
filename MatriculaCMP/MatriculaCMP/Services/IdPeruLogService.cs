using System.Text;

namespace MatriculaCMP.Services
{
    /// <summary>
    /// Servicio simple para loguear eventos de ID Perú a archivo.
    /// Los logs se guardan en: [RaizAplicación]/Logs/idperu-YYYY-MM-DD.log
    /// </summary>
    public class IdPeruLogService
    {
        private readonly string _logDirectory;
        private readonly object _lock = new();

        public IdPeruLogService(IWebHostEnvironment env)
        {
            _logDirectory = Path.Combine(env.ContentRootPath, "Logs");
            
            // Crear directorio si no existe
            if (!Directory.Exists(_logDirectory))
            {
                Directory.CreateDirectory(_logDirectory);
            }
        }

        public void Log(string level, string message, params object[] args)
        {
            try
            {
                var formattedMessage = args.Length > 0 ? string.Format(message, args) : message;
                var logLine = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss.fff}] [{level}] {formattedMessage}";
                var logFile = Path.Combine(_logDirectory, $"idperu-{DateTime.Now:yyyy-MM-dd}.log");

                lock (_lock)
                {
                    File.AppendAllText(logFile, logLine + Environment.NewLine, Encoding.UTF8);
                }
            }
            catch
            {
                // Si falla el logging, no queremos que rompa la aplicación
            }
        }

        public void Info(string message, params object[] args) => Log("INFO", message, args);
        public void Warn(string message, params object[] args) => Log("WARN", message, args);
        public void Error(string message, params object[] args) => Log("ERROR", message, args);
        public void Debug(string message, params object[] args) => Log("DEBUG", message, args);
    }
}
