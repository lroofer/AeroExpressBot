using Microsoft.Extensions.Logging;

namespace FileProcessing;

/// <summary>
/// Custom provider for logging to the file.
/// </summary>
public class FileLoggerProvider : ILoggerProvider
{
    private readonly string _logFilePath;

    public FileLoggerProvider(string logFileName)
    {
        // Generate log file path
        _logFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, logFileName);
    }

    public ILogger CreateLogger(string categoryName)
    {
        return new FileLogger(_logFilePath);
    }

    public void Dispose()
    {
    }
}

public class FileLogger : ILogger
{
    private readonly string _logFilePath;
    private readonly object _lock = new object();

    public FileLogger(string logFilePath)
    {
        _logFilePath = logFilePath;
    }

    public bool IsEnabled(LogLevel logLevel)
    {
        return true;
    }

    public IDisposable? BeginScope<TState>(TState state) where TState : notnull
    {
        return null;
    }

    public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception,
        Func<TState, Exception, string> formatter)
    {
        lock (_lock)
        {
            File.AppendAllText(_logFilePath,
                $"{DateTime.Now} [{logLevel}] - {formatter(state, exception ?? new Exception("None"))}{Environment.NewLine}");
        }
    }
}