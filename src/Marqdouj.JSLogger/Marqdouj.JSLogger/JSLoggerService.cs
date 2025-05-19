using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.JSInterop;
using System.Text;

namespace Marqdouj.JSLogger
{
    public interface IJSLoggerService : IJSLogger
    {
    }

    public class JSLoggerService(
            IJSRuntime jsRuntime,
            IJSLoggerConfig config) : IJSLoggerService
    {
        private readonly IJSRuntime jsRuntime = jsRuntime;

        /// <summary>
        /// Flag to log the exception details.
        /// </summary>
        public bool DetailedErrors { get; set; } = true;

        public IJSLoggerConfig Config { get; } = (IJSLoggerConfig)config.Clone();

        public bool IsEnabled(LogLevel logLevel) => Config.IsEnabled(logLevel);

        public async ValueTask LogRaw(string message, string style = "")
        {
            await jsRuntime.InvokeVoidAsync("MarqdoujJsl.Logger.logRaw", message, style);
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

            var logEvent = BuildLogEventIdentifier(logLevel);
            await jsRuntime.InvokeVoidAsync(logEvent, Config, message, eventId);
        }

        public async ValueTask Test(string message = "")
        {
            await jsRuntime.InvokeVoidAsync("MarqdoujJsl.test", Config, message);
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
            var path = $"MarqdoujJsl.Logger.log{logLevelName}";
            return path;
        }

    }

    public static class LoggerServiceExtensions
    {
        public static IHostApplicationBuilder AddLoggerService(this IHostApplicationBuilder builder, IJSLoggerConfig config)
        {
            builder.Services.AddSingleton(config);
            builder.Services.AddScoped<IJSLoggerService, JSLoggerService>();
            return builder;
        }
    }
}