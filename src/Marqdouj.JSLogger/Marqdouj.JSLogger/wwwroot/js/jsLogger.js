export var LogLevel;
(function (LogLevel) {
    LogLevel[LogLevel["trace"] = 0] = "trace";
    LogLevel[LogLevel["debug"] = 1] = "debug";
    LogLevel[LogLevel["information"] = 2] = "information";
    LogLevel[LogLevel["warn"] = 3] = "warn";
    LogLevel[LogLevel["error"] = 4] = "error";
    LogLevel[LogLevel["critical"] = 5] = "critical";
    LogLevel[LogLevel["none"] = 6] = "none";
})(LogLevel || (LogLevel = {}));
export function test(config, message = 'Testing Logger') {
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
export function formatMessage(template, category, level, event, message) {
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
function checkMessage(message) {
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
export class LoggerConfig {
    #category;
    #minLevel;
    #maxLevel;
    #template;
    constructor(category, minLevel = LogLevel.information, maxLevel = LogLevel.critical, template = "") {
        this.#category = category;
        this.#template = template.length == 0 ? "{category}{event}{timestamp}{level}: {message}" : template;
        this.#setLevel(minLevel, maxLevel);
    }
    get category() {
        return this.#category;
    }
    get minLevel() {
        return this.#minLevel;
    }
    get maxLevel() {
        return this.#maxLevel;
    }
    get template() {
        return this.#template;
    }
    #setLevel(min, max) {
        if (min > max) {
            throw (`Minimum log level ${min} cannot be greater than maximum log level ${max}.`);
        }
        this.#minLevel = min;
        this.#maxLevel = max;
    }
}
export class Logger {
    static logTrace(config, message, event = "") {
        this.log(config, LogLevel.trace, message, event);
    }
    static logDebug(config, message, event = "") {
        this.log(config, LogLevel.debug, message, event);
    }
    static logInformation(config, message, event = "") {
        this.log(config, LogLevel.information, message, event);
    }
    static logWarning(config, message, event = "") {
        this.log(config, LogLevel.warn, message, event);
    }
    static logError(config, message, event = "") {
        this.log(config, LogLevel.error, message, event);
    }
    static logCritical(config, message, event = "") {
        this.log(config, LogLevel.critical, message, event);
    }
    static isEnabled(config, level) {
        return level != LogLevel.none && level >= config.minLevel && level <= config.maxLevel;
    }
    static logRaw(config, level, message) {
        this.logMessage(config, level, message);
    }
    static log(config, level, message, event = "") {
        let fMessage = formatMessage(config.template, config.category, level, event, message);
        this.logMessage(config, level, fMessage);
    }
    static logMessage(config, level, message) {
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
