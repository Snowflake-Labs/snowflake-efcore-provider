using log4net;
using Microsoft.Extensions.Logging;
using System.Collections.Concurrent;
using log4net.Config;
using Microsoft.Extensions.DependencyInjection;

namespace EFCore.Tests;

internal class Log4NetLogger : ILogger
{
    public string LoggerName { get; }

    internal ILog Logger { get; set; }

    public Log4NetLogger(string loggerName)
    {
        if (string.IsNullOrWhiteSpace(loggerName))
        {
            throw new ArgumentNullException(nameof(loggerName));
        }

        LoggerName = loggerName;
        Logger = LogManager.GetLogger(loggerName);
    }

    public bool IsEnabled(LogLevel logLevel)
    {
        switch (logLevel)
        {
            case LogLevel.Trace:
            case LogLevel.Debug:
                    return Logger.IsDebugEnabled;
            case LogLevel.Information:
                    return Logger.IsInfoEnabled;
            case LogLevel.Warning:
                    return Logger.IsWarnEnabled;
            case LogLevel.Error:
                    return Logger.IsErrorEnabled;
            case LogLevel.Critical:
                    return Logger.IsFatalEnabled;
            default:
                    return false;
        }
    }

    public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
    {
        if (!IsEnabled(logLevel))
            return;

        var message = formatter == null ? state.ToString() : formatter(state, exception);

        switch (logLevel)
        {
            case LogLevel.Trace:
            case LogLevel.Debug:
                    Logger.Debug(message, exception);
                    break;

            case LogLevel.Information:
                    Logger.Info(message, exception);
                    break;
            case LogLevel.Warning:
                    Logger.Warn(message, exception);
                    break;
            case LogLevel.Error:
                    Logger.Error(message, exception);
                    break;
            case LogLevel.Critical:
                    Logger.Fatal(message, exception);
                    break;
        }
    }

    public IDisposable BeginScope<TState>(TState state)
    {
        return null;
    }
}

internal class Log4NetProvider : ILoggerProvider
{
    private ConcurrentDictionary<string, Lazy<ILogger>> Loggers { get; } 

    public Log4NetProvider()
    {
        Loggers = new ConcurrentDictionary<string, Lazy<ILogger>>();
    }

    public ILogger CreateLogger(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentNullException(nameof(name));
        }

        var logger = Loggers.GetOrAdd(name,
            loggerName =>
                new Lazy<ILogger>(() => new Log4NetLogger(loggerName), LazyThreadSafetyMode.ExecutionAndPublication));

        return logger.Value;         
    }
    
    public void Dispose()
    {
        Loggers.Clear();            
    }
}

public static class Log4NetFactory
{
    private static bool s_Initialized = false;

    public static ILogger Logger(string loggerName, string configuration = "app.config") => GetLogger(loggerName, configuration);

    public static ILogger Logger(Type type, string configuration = "app.config") => GetLogger(type.ToString(), configuration);

    private static ILogger GetLogger(string loggerName, string configuration)
    {
        LoggerInit(configuration);
        return new Log4NetLogger(loggerName);
    }

    private static ILoggerProvider LoggerProvider() => new Log4NetProvider();

    internal static void LoggerInit(string configuration)
    {
        if (string.IsNullOrWhiteSpace(configuration))
            throw new ArgumentNullException(nameof(configuration));
        if (s_Initialized)
            return;
        var filePath = new FileInfo(configuration);
        XmlConfigurator.Configure(filePath);
        s_Initialized = true;
    }

    internal static ILoggerFactory LoggerFactory()
    {
        var serviceProvider = new ServiceCollection().AddLogging().BuildServiceProvider();
        if (serviceProvider == null)
            throw new ArgumentNullException(nameof(serviceProvider));
        var loggerFactory = serviceProvider.GetService<ILoggerFactory>();
        if (loggerFactory == null)
            throw new ArgumentNullException(nameof(loggerFactory));
        var loggerProvider = LoggerProvider();
        loggerFactory.AddProvider(loggerProvider);
        return (LoggerFactory)loggerFactory;
    }
 }
