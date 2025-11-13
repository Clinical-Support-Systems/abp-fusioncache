using Abp.Dependency;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace CommunityAbp.AspNetZero.FusionCache.Internal
{
    // <summary>
    /// Factory that creates Microsoft.Extensions.Logging.ILogger instances 
    /// backed by ABP's Castle.Core.Logging.ILogger system.
    /// </summary>
    public class AbpFusionCacheLoggerFactory : ILoggerFactory, ISingletonDependency
    {
        private readonly ILoggerFactory _abpLoggerFactory;

        public AbpFusionCacheLoggerFactory()
        {
            // ABP will inject the logger factory
            _abpLoggerFactory = NullLoggerFactory.Instance;
        }

        public AbpFusionCacheLoggerFactory(ILoggerFactory loggerFactory)
        {
            _abpLoggerFactory = loggerFactory ?? NullLoggerFactory.Instance;
        }

        // Required property for ABP dependency injection
        public ILoggerFactory LoggerFactory
        {
            get => _abpLoggerFactory;
            set => throw new NotSupportedException("LoggerFactory cannot be set after construction");
        }

        public void AddProvider(ILoggerProvider provider)
        {
            // ABP manages providers differently, this is a no-op
        }

        public Microsoft.Extensions.Logging.ILogger CreateLogger(string categoryName)
        {
            var abpLogger = _abpLoggerFactory.CreateLogger(categoryName);
            return new AbpFusionCacheLogger(abpLogger);
        }

        public void Dispose()
        {
            // ABP manages logger lifecycle
        }
    }

    public class AbpFusionCacheLogger : ILogger, ISingletonDependency
    {
        private readonly ILogger _logger;
        private const string CategoryName = "FusionCache";

        public AbpFusionCacheLogger(ILogger logger)
        {
            _logger = logger;
        }

        public IDisposable BeginScope<TState>(TState state)
            where TState : notnull
        {
            return _logger?.BeginScope(state) ?? NullDisposable.Instance;
        }

        public bool IsEnabled(LogLevel logLevel)
        {
            return _logger?.IsEnabled(logLevel) ?? false;
        }

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception, string> formatter)
        {
            if (!IsEnabled(logLevel))
                return;

            var message = formatter(state, exception);
            var fullMessage = $"[{CategoryName}] {message}";

            // Map to ABP logging levels
            switch (logLevel)
            {
                case LogLevel.Trace:
                case LogLevel.Debug:
                    _logger?.LogDebug(eventId, exception, fullMessage);
                    break;
                case LogLevel.Information:
                    _logger?.LogInformation(eventId, exception, fullMessage);
                    break;
                case LogLevel.Warning:
                    _logger?.LogWarning(eventId, exception, fullMessage);
                    break;
                case LogLevel.Error:
                    _logger?.LogError(eventId, exception, fullMessage);
                    break;
                case LogLevel.Critical:
                    _logger?.LogCritical(eventId, exception, fullMessage);
                    break;
                default:
                    _logger?.LogInformation(eventId, exception, fullMessage);
                    break;
            }
        }

        private class NullDisposable : IDisposable
        {
            public static readonly NullDisposable Instance = new();
            public void Dispose() { }
        }
    }
}
