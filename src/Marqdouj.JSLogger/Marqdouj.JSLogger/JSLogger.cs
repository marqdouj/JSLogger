using Microsoft.Extensions.Logging;
using Microsoft.JSInterop;
using System.Text;

namespace Marqdouj.JSLogger
{
    public interface IJSLogger<T> : IJSLogger where T : class
    {

    }

    public interface IJSLogger
    {
        IJSLoggerConfig Config { get; set; }
        bool DetailedErrors { get; set; }

        ValueTask DisposeAsync();

        IDisposable BeginScope<TState>(TState state);

        bool IsEnabled(LogLevel logLevel);
        ValueTask Log(LogLevel logLevel, string message, string eventId = "");
        ValueTask LogCritical(string message, string eventId = "");
        ValueTask LogDebug(string message, string eventId = "");
        ValueTask LogError(Exception exception, string eventId = "");
        ValueTask LogError(string message, string eventId = "");
        ValueTask LogInformation(string message, string eventId = "");
        ValueTask LogRaw(string message, string style = "");
        ValueTask LogTrace(string message, string eventId = "");
        ValueTask LogWarning(string message, string eventId = "");
        ValueTask Test(string message = "");
    }

    public class JSLogger<T> : JSLogger, IJSLogger<T> where T : class
    {
        public JSLogger(IJSRuntime jsRuntime) : base(jsRuntime)
        {
            this.Config.Category = typeof(T).Name;
        }

        public JSLogger(IJSRuntime jsRuntime, IJSLoggerConfig config) : base(jsRuntime, config)
        {
            this.Config.Category = typeof(T).Name;
        }
    }

    public partial class JSLogger(IJSRuntime jsRuntime) : IAsyncDisposable, IJSLogger
    {
        private readonly Lazy<Task<IJSObjectReference>> moduleTask = new(() => jsRuntime.InvokeAsync<IJSObjectReference>(
                "import", "./_content/Marqdouj.JSLogger/js/jsLogger.js").AsTask());
        private IJSLoggerConfig config = new JSLoggerConfig();

        private static readonly AsyncLocal<Stack<string?>?> _scopes = new();

        public JSLogger(IJSRuntime jsRuntime, IJSLoggerConfig config) : this(jsRuntime)
        {
            Config = config ?? throw new ArgumentNullException(nameof(config));
        }

        public IJSLoggerConfig Config { get => config; set { ArgumentNullException.ThrowIfNull(value, nameof(Config)); config = value; } }

        public bool IsEnabled(LogLevel logLevel) => Config.IsEnabled(logLevel);

        /// <summary>
        /// Flag to include full exception details when logging an exception.
        /// </summary>
        public bool DetailedErrors { get; set; } = true;

        public async ValueTask LogRaw(string message, string style = "")
        {
            var module = await moduleTask.Value;
            await module.InvokeVoidAsync("Logger.logRaw", message, style);
        }

        public async ValueTask LogTrace(string message, string eventId = "")
        {
            await Log(LogLevel.Trace, message, eventId);
        }

        public async ValueTask LogDebug(string message, string eventId = "")
        {
            await Log(LogLevel.Debug, message, eventId);
        }

        public async ValueTask LogInformation(string message, string eventId = "")
        {
            await Log(LogLevel.Information, message, eventId);
        }

        public async ValueTask LogWarning(string message, string eventId = "")
        {
            await Log(LogLevel.Warning, message, eventId);
        }

        public async ValueTask LogError(string message, string eventId = "")
        {
            await Log(LogLevel.Error, message, eventId);
        }

        public async ValueTask LogError(Exception exception, string eventId = "")
        {

            var sb = new StringBuilder();
            var formattedMessage = exception.Message;

            if (!string.IsNullOrWhiteSpace(formattedMessage))
                sb.AppendLine(formattedMessage);

            if (DetailedErrors && exception != null)
                sb.AppendLine(exception.ToString());

            var message = sb.ToString();

            await Log(LogLevel.Error, message, eventId);
        }

        public async ValueTask LogCritical(string message, string eventId = "")
        {
            await Log(LogLevel.Critical, message, eventId);
        }

        public async ValueTask Log(LogLevel logLevel, string message, string eventId = "")
        {
            if (!IsEnabled(logLevel))
                return;

            var logEvent = logLevel.BuildLogEventIdentifier("");
            var module = await moduleTask.Value;

            await module.InvokeVoidAsync(logEvent, Config, message, eventId);
        }

        public async ValueTask Test(string message = "")
        {
            var module = await moduleTask.Value;
            await module.InvokeVoidAsync("test", Config, message);
        }

        public async ValueTask DisposeAsync()
        {
            try
            {
                if (moduleTask.IsValueCreated)
                {
                    var module = await moduleTask.Value;
                    await module.DisposeAsync();
                }
            }
            catch (JSDisconnectedException)
            {
            }
            finally
            {
                // Suppress finalization to comply with CA1816
                GC.SuppressFinalize(this);
            }
        }

        public IDisposable BeginScope<TState>(TState state)
        {
            _scopes.Value ??= new Stack<string?>();
            _scopes.Value.Push(state?.ToString());

            return new LoggerScope(() =>
            {
                _scopes.Value.Pop();
                if (_scopes.Value.Count == 0)
                {
                    _scopes.Value = null;
                }
            });
        }
    }
}
