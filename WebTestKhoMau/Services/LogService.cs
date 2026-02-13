using System.Text.Json;

namespace WebTestKhoMau.Services
{
    public interface ILogService
    {
        Task LogApiCallAsync(string apiName, object? input, object? output, string status, string? errorMessage = null);
    }

    public class LogService : ILogService
    {
        private readonly IWebHostEnvironment _env;
        private readonly ILogger<LogService> _logger;
        private readonly string _logsDirectory;

        public LogService(IWebHostEnvironment env, ILogger<LogService> logger)
        {
            _env = env;
            _logger = logger;
            _logsDirectory = Path.Combine(env.ContentRootPath, "Logs");
            
            // Create Logs directory if it doesn't exist
            if (!Directory.Exists(_logsDirectory))
            {
                Directory.CreateDirectory(_logsDirectory);
                _logger.LogInformation("Created Logs directory");
            }
        }

        public async Task LogApiCallAsync(string apiName, object? input, object? output, string status, string? errorMessage = null)
        {
            try
            {
                var timestamp = DateTime.Now;
                var fileName = $"API_{apiName}_{timestamp:yyyy-MM-dd}.log";
                var filePath = Path.Combine(_logsDirectory, fileName);

                var logEntry = new
                {
                    Time = timestamp.ToString("yyyy-MM-dd HH:mm:ss.fff"),
                    API = apiName,
                    Status = status,
                    Input = input,
                    Output = output,
                    ErrorMessage = errorMessage ?? ""
                };

                var logJson = JsonSerializer.Serialize(logEntry, new JsonSerializerOptions { WriteIndented = true });
                var logLine = $"\n{new string('=', 80)}\n{logJson}\n{new string('=', 80)}";

                // Append to file asynchronously
                await File.AppendAllTextAsync(filePath, logLine);

                _logger.LogDebug($"API log written to {fileName}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error writing API log");
            }
        }
    }
}
