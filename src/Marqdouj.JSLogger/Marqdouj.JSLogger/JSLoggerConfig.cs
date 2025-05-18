using Microsoft.Extensions.Logging;

namespace Marqdouj.JSLogger
{
    public interface IJSLoggerConfig : ICloneable
    {
        string DefaultTemplate { get; set; }
        string Category { get; }
        bool IsEnabled(LogLevel logLevel);
        LogLevel MaxLevel { get; }
        LogLevel MinLevel { get; }
        string Template { get; }

        void SetLevel(LogLevel min, LogLevel max);
    }

    public class JSLoggerConfig : IJSLoggerConfig
    {
        private static string defaultTemplate = "{category}{event}{timestamp}{level}: {message}";

        public string DefaultTemplate
        {
            get => defaultTemplate;
            set
            {
                ArgumentNullException.ThrowIfNullOrWhiteSpace(value, nameof(defaultTemplate));
                defaultTemplate = value;
            }
        }

        public JSLoggerConfig(string category = "", LogLevel min = LogLevel.Information, LogLevel max = LogLevel.Critical, string template = "")
        {
            Category = category;
            Template = string.IsNullOrWhiteSpace(template) ? DefaultTemplate : template;
            SetLevel(min, max);
        }

        public string Category { get; }
        public LogLevel MinLevel { get; private set; } = LogLevel.Information;
        public LogLevel MaxLevel { get; private set; } = LogLevel.Critical;
        public string Template { get; }

        public bool IsEnabled(LogLevel logLevel)
        {
            return logLevel != LogLevel.None && logLevel >= MinLevel && logLevel <= MaxLevel;
        }

        public void SetLevel(LogLevel min, LogLevel max)
        {
            if (min > max)
            {
                throw new ArgumentException($"Minimum log level {min} cannot be greater than maximum log level {max}.");
            }
            MinLevel = min;
            MaxLevel = max;
        }

        public object Clone()
        {
            return MemberwiseClone();
        }
    }
}
