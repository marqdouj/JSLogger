export enum LogLevel {
    trace = 0,
    debug = 1,
    information = 2,
    warn = 3,
    error = 4,
    critical = 5,
    none = 6,
}

export function test(config: ILoggerConfig, message: string = 'Testing Logger') {
    const event = 'testLogger';

    config = config || new LoggerConfig('test', LogLevel.trace, LogLevel.critical);

    console.log(`${event}: Template [${config.template}]`);

    Logger.logTrace(config, message, event);
    Logger.logDebug(config, message, event);
    Logger.logInformation(config, message, event);
    Logger.logWarning(config, message, event);
    Logger.logError(config, message, event);
    Logger.logCritical(config, message, event);
}

export function formatMessage(template: string, category: string, level: LogLevel, event: string, message: string) {
    let tCategory = template.includes("{category}") ? `${category} ` : '';
    let tLevel = template.includes("{level}") ? `${LogLevel[level]} ` : '';
    let tEvent = template.includes("{event}") ? `${event} ` : '';
    let tTimestamp = template.includes("{timestamp}") ? `${new Date().toISOString()} ` : '';
    let tMessage = template.includes("{message}") ? `${checkMessage(message)}` : '';

    var tTemplate = template
        .replace("{category}", tCategory)
        .replace("{level}", tLevel)
        .replace("{event}", tEvent)
        .replace("{timestamp}", tTimestamp)
        .replace("{message}", tMessage);

    return tTemplate;
}

function checkMessage(message: string): string {
    let messageCheck = '';

    if (typeof message !== 'string') {
        messageCheck = 'log requested but message is not a string';
    }

    if (message.length === 0) {
        if (messageCheck.length > 0) {
            messageCheck += '; ';
        }
        messageCheck += 'log requested but message is empty';
    }

    return messageCheck.length > 0 ? messageCheck : message;
}

export interface ILoggerConfig {
    get category(): string;
    get minLevel(): LogLevel;
    get maxLevel(): LogLevel;
    get template(): string;
}

export class LoggerConfig {
    #category: string;
    #minLevel: LogLevel;
    #maxLevel: LogLevel;
    #template: string;

    constructor(category: string, minLevel: LogLevel = LogLevel.information, maxLevel: LogLevel = LogLevel.critical, template: string = "") {
        this.#category = category;
        this.#template = template.length == 0 ? "{category}{event}{timestamp}{level}: {message}" : template;
        this.#setLevel(minLevel, maxLevel);
    }

    get category(): string {
        return this.#category;
    }

    get minLevel(): LogLevel {
        return this.#minLevel;
    }

    get maxLevel(): LogLevel {
        return this.#maxLevel;
    }

    get template(): string {
        return this.#template;
    }

    #setLevel(min: LogLevel, max: LogLevel) {
        if (min > max) {
            throw (`Minimum log level ${min} cannot be greater than maximum log level ${max}.`);
        }

        this.#minLevel = min;
        this.#maxLevel = max;
    }
}

export class Logger {
    public static logTrace(config: ILoggerConfig, message: string, event: string = ""): void {
        this.log(config, LogLevel.trace, message, event);
    }

    public static logDebug(config: ILoggerConfig, message: string, event: string = ""): void {
        this.log(config, LogLevel.debug, message, event);
    }

    public static logInformation(config: ILoggerConfig, message: string, event: string = ""): void {
        this.log(config, LogLevel.information, message, event);
    }

    public static logWarning(config: ILoggerConfig, message: string, event: string = ""): void {
        this.log(config, LogLevel.warn, message, event);
    }

    public static logError(config: ILoggerConfig, message: string, event: string = ""): void {
        this.log(config, LogLevel.error, message, event);
    }

    public static logCritical(config: ILoggerConfig, message: string, event: string = ""): void {
        this.log(config, LogLevel.critical, message, event);
    }

    private static isEnabled(config: ILoggerConfig, level: LogLevel): boolean {
        return level != LogLevel.none && level >= config.minLevel && level <= config.maxLevel;
    }

    public static logRaw(message: string, style: string = ""): void {
        if (typeof message !== 'string') {
            message = 'log requested but message is not a string';
        }
        if (message.length === 0) {
            message = 'log requested but message is empty';
        }
        console.log(`${"%c"}${message}`, style);
    }

    public static log(config: ILoggerConfig, level: LogLevel, message: string, event: string = ""): void {
        let fMessage = formatMessage(config.template, config.category, level, event, message);
        this.logMessage(config, level, fMessage);
    }

    private static logMessage(config: ILoggerConfig, level: LogLevel, message: string): void {
        if (this.isEnabled(config, level)) {
            switch (level) {
                case LogLevel.trace:
                    console.trace(message);
                    break;
                case LogLevel.debug:
                    console.debug(message);
                    break;
                case LogLevel.information:
                    console.info(message);
                    break;
                case LogLevel.warn:
                    console.warn(message);
                    break;
                case LogLevel.error:
                    console.error(message);
                    break;
                case LogLevel.critical:
                    console.error(message);
                    break;
                case LogLevel.none:
                default:
                    // No logging
                    break;
            }
        }
    }
}