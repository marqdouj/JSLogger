using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.JSInterop;
using System.Text;

namespace Marqdouj.JSLogger
{
    public interface IJSLoggerService<T> : IJSLoggerService where T : class
    {
    }

    public interface IJSLoggerService : IJSLogger
    {
    }

    public class JSLoggerService<T> : JSLoggerService, IJSLoggerService<T> where T : class
    {
        public JSLoggerService(IJSRuntime jsRuntime) : base(jsRuntime)
        {
            this.Config.Category = typeof(T).Name;
        }

        public JSLoggerService(IJSRuntime jsRuntime, IJSLoggerConfig config) : base(jsRuntime, config)
        {
            this.Config.Category = typeof(T).Name;
        }
    }

    public class JSLoggerService(IJSRuntime jsRuntime) : IAsyncDisposable, IJSLoggerService
    {
        private readonly IJSRuntime jsRuntime = jsRuntime;
        private IJSLoggerConfig config = new JSLoggerConfig();
        private const string libName = "MarqdoujJsl";

        public JSLoggerService(IJSRuntime jsRuntime, IJSLoggerConfig config) : this(jsRuntime)
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
            await jsRuntime.InvokeVoidAsync($"{libName}.Logger.logRaw", message, style);
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
            await jsRuntime.InvokeVoidAsync($"{libName}.test", Config, message);
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
            var path = $"{libName}.Logger.log{logLevelName}";
            return path;
        }

        public async ValueTask DisposeAsync()
        {
            //required for IJSLogger
            await Task.CompletedTask;
        }
    }

    public static class LoggerServiceExtensions
    {
        public static IServiceCollection AddLoggerService(this IServiceCollection services, IJSLoggerConfig? config)
        {
            if (config is not null)
            {
                services.AddSingleton(config);
            }

            services.AddScoped<IJSLoggerService, JSLoggerService>();
            services.AddScoped(typeof(IJSLoggerService<>), typeof(JSLoggerService<>));

            return services;
        }
        
        public static IHostApplicationBuilder AddLoggerService(this IHostApplicationBuilder builder, IJSLoggerConfig? config)
        {
            builder.Services.AddLoggerService(config);
            return builder;
        }
    }
}