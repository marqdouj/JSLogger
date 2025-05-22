using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.Text;

namespace Marqdouj.JSLogger
{
    public static class LoggerExtensions
    {
        public static IServiceCollection AddLoggerModule(this IServiceCollection services, IJSLoggerConfig? config)
        {
            if (config is not null)
            {
                services.AddSingleton(config);
            }

            services.AddScoped<IJSLogger, JSLogger>();
            services.AddScoped(typeof(IJSLogger<>), typeof(JSLogger<>));

            return services;
        }

        public static IHostApplicationBuilder AddLoggerModule(this IHostApplicationBuilder builder, IJSLoggerConfig? config)
        {
            builder.Services.AddLoggerModule(config);
            return builder;
        }

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

        internal static string BuildLogEventIdentifier(this LogLevel logLevel, string libName)
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

            if (!string.IsNullOrEmpty(libName))
                libName = $"{libName}.";

            var path = $"{libName}Logger.log{logLevelName}";
            return path;
        }

        internal static string ToMessage<TState>(this IJSLogger logger, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
        {
            var sb = new StringBuilder();
            var formattedMessage = formatter(state, exception);

            if (!string.IsNullOrWhiteSpace(formattedMessage))
                sb.AppendLine(formattedMessage);

            if (logger.DetailedErrors && exception != null)
                sb.AppendLine(exception.ToString());

            var message = sb.ToString();

            return message;
        }
    }
}
