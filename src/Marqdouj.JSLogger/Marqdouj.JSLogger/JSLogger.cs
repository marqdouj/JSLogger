using Microsoft.Extensions.Logging;
using Microsoft.JSInterop;
using System.Text;

namespace Marqdouj.JSLogger
{
    public class JSLogger<T>(
        IJSRuntime jsRuntime,
        LogLevel min = LogLevel.Information,
        LogLevel max = LogLevel.Critical,
        string template = "") : JSLogger(jsRuntime, typeof(T).Name, min, max, template) where T : class
    {
    }

    public interface IJSLogger
    {
        IJSLoggerConfig Config { get; }
        bool DetailedErrors { get; set; }

        bool IsEnabled(LogLevel logLevel);
        ValueTask Log(LogLevel logLevel, string message, string eventId = "");
        ValueTask LogCritical(string message, string eventId = "");
        ValueTask LogDebug(string message, string eventId = "");
        ValueTask LogError(Exception exception, string eventId = "");
        ValueTask LogError(string message, string eventId = "");
        ValueTask LogInformation(string message, string eventId = "");
        ValueTask LogTrace(string message, string eventId = "");
        ValueTask LogWarning(string message, string eventId = "");
        ValueTask Test(string message = "");
    }

    public class JSLogger(
        IJSRuntime jsRuntime,
        string category,
        LogLevel min = LogLevel.Information,
        LogLevel max = LogLevel.Critical,
        string template = "") : IAsyncDisposable, IJSLogger
    {
        private readonly Lazy<Task<IJSObjectReference>> moduleTask = new(() => jsRuntime.InvokeAsync<IJSObjectReference>(
                "import", "./_content/Marqdouj.JSLogger/js/jsLogger.js").AsTask());

        public IJSLoggerConfig Config { get; init; } = new JSLoggerConfig(category, min, max, template);

        public bool IsEnabled(LogLevel logLevel) => Config.IsEnabled(logLevel);

        /// <summary>
        /// Flag to log the exception details.
        /// </summary>
        public bool DetailedErrors { get; set; } = true;

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

            var logEvent = BuildLogEventIdentifier(logLevel);
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
            if (moduleTask.IsValueCreated)
            {
                var module = await moduleTask.Value;
                await module.DisposeAsync();
            }
        }

        private static string BuildLogEventIdentifier(LogLevel logLevel)
        {
            string? logLevelName = logLevel switch
            {
                LogLevel.Trace => "Trace",
                LogLevel.Debug => "Debug",
                LogLevel.Information => "Information",
                LogLevel.Warning => "Warning",
                LogLevel.Error => "Error",
                LogLevel.Critical => "Critical",
                _ => throw new ArgumentOutOfRangeException(nameof(logLevel), logLevel, "LogLevel not supported for logging."),
            };
            var path = $"Logger.log{logLevelName}";
            return path;
        }
    }
}
