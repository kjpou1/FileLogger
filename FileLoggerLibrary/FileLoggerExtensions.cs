﻿using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace FileLoggerLibrary;

public static class FileLoggerExtensions
{
    public static ILoggingBuilder AddFileLogger(this ILoggingBuilder builder, string name, bool append = true)
    {
        builder.Services.AddSingleton<ILoggerProvider, FileLoggerProvider>(sp => new FileLoggerProvider(name, append: append));
        builder.SetMinimumLevel(LogLevel.Trace);
        return builder;
    }

    public static ILoggingBuilder AddFileLogger(this ILoggingBuilder builder, string name, LogLevel minLevel, bool append = true)
    {
        builder.Services.AddSingleton<ILoggerProvider, FileLoggerProvider>(sp => new FileLoggerProvider(name, append: append));
        builder.SetMinimumLevel(minLevel);
        return builder;
    }

    public static ILoggingBuilder AddFileLogger(this ILoggingBuilder builder, string name, string folder, bool append = true)
    {
        builder.Services.AddSingleton<ILoggerProvider, FileLoggerProvider>(sp => new FileLoggerProvider(name, folder, append: append));
        builder.SetMinimumLevel(LogLevel.Trace);
        return builder;
    }
    
    public static ILoggingBuilder AddFileLogger(this ILoggingBuilder builder, string name, string folder, LogLevel minLevel, bool append = true)
    {
        builder.Services.AddSingleton<ILoggerProvider, FileLoggerProvider>(sp => new FileLoggerProvider(name, folder, append: append));
        builder.SetMinimumLevel(minLevel);
        return builder;
    }

    public static ILoggingBuilder AddFileLogger(this ILoggingBuilder builder, string name, string folder, long maxBytes, bool append = true)
    {
        builder.Services.AddSingleton<ILoggerProvider, FileLoggerProvider>(sp => new FileLoggerProvider(name, folder, maxBytes, append: append));
        builder.SetMinimumLevel(LogLevel.Trace);
        return builder;
    }

    public static ILoggingBuilder AddFileLogger(this ILoggingBuilder builder, string name, string folder, long maxBytes, uint maxCount, bool append = true)
    {
        builder.Services.AddSingleton<ILoggerProvider, FileLoggerProvider>(sp => new FileLoggerProvider(name, folder, maxBytes, maxCount, append: append));
        builder.SetMinimumLevel(LogLevel.Trace);
        return builder;
    }

    public static ILoggingBuilder AddFileLogger(this ILoggingBuilder builder, string name, string folder, long maxBytes, uint maxCount, LogLevel minLevel, bool append = true)
    {
        builder.Services.AddSingleton<ILoggerProvider, FileLoggerProvider>(sp => new FileLoggerProvider(name, folder, maxBytes, maxCount, minLevel, append: append));
        builder.SetMinimumLevel(minLevel);
        return builder;
    }

    public static ILoggingBuilder AddFileLogger(this ILoggingBuilder builder, Action<FileLoggerOptions> configure)
    {
        FileLoggerOptions options = new();
        configure(options);
        builder.Services.AddSingleton<ILoggerProvider, FileLoggerProvider>(sp => new FileLoggerProvider(options));
        builder.SetMinimumLevel(options.LogMinLevel);
        return builder;
    }

    public static ILoggingBuilder AddFileLogger(this ILoggingBuilder builder, IConfiguration configuration, Action<FileLoggerOptions> configure = null)
    {
        FileLoggerProvider fileLoggerProvider = CreateFromConfiguration(configuration, configure = null);

        if (fileLoggerProvider != null)
        {
            builder.Services.AddSingleton<ILoggerProvider, FileLoggerProvider>(sp => fileLoggerProvider);
        }

        builder.SetMinimumLevel(fileLoggerProvider.LogMinLevel);
        return builder;
    }

    private static FileLoggerProvider CreateFromConfiguration(IConfiguration configuration, Action<FileLoggerOptions> configure)
    {
        IConfigurationSection fileLogger = configuration.GetSection("Logging:FileLogger");

        if (fileLogger == null)
        {
            return null;
        }

        FileLoggerOptions options = new();

        string logName = fileLogger["LogName"];

        if (string.IsNullOrWhiteSpace(logName) == false)
        {
            options.LogName = logName;
        }
        else
        {
            return null;
        }

        string logFolder = fileLogger["LogFolder"];

        if (string.IsNullOrWhiteSpace(logFolder) == false)
        {
            options.LogFolder = logFolder;
        }

        string logMaxBytes = fileLogger["LogMaxBytes"];

        if (string.IsNullOrWhiteSpace(logMaxBytes) == false && long.TryParse(logMaxBytes, out long maxBytes))
        {
            options.LogMaxBytes = maxBytes;
        }

        string logMaxCount = fileLogger["LogMaxCount"];

        if (string.IsNullOrWhiteSpace(logMaxCount) == false && uint.TryParse(logMaxCount, out uint maxFiles))
        {
            options.LogMaxCount = maxFiles;
        }

        string minLevel = fileLogger["LogMinLevel"];
        
        if (string.IsNullOrWhiteSpace(minLevel) == false && Enum.TryParse(minLevel, out LogLevel level))
        {
            options.LogMinLevel = level;
        }

        string multiLineFormat = fileLogger["MultilineFormat"];

        if (string.IsNullOrWhiteSpace(multiLineFormat) == false && bool.TryParse(multiLineFormat, out bool multiLine))
        {
            options.MultiLineFormat = multiLine;
        }

        string append = fileLogger["Append"];

        if (string.IsNullOrWhiteSpace(append) == false && bool.TryParse(append, out bool appendTo))
        {
            options.Append = appendTo;
        }

        string indentMultilineMessages = fileLogger["IndentMultilineMessages"];

        if (string.IsNullOrWhiteSpace(indentMultilineMessages) == false && bool.TryParse(indentMultilineMessages, out bool indent))
        {
            options.IndentMultilineMessages = indent;
        }

        string consoleLogging = fileLogger["ConsoleLogging"];

        if (string.IsNullOrWhiteSpace(consoleLogging) == false && bool.TryParse(consoleLogging, out bool console))
        {
            options.ConsoleLogging = console;
        }

        string enableConsoleColors = fileLogger["EnableConsoleColors"];

        if (string.IsNullOrWhiteSpace(enableConsoleColors) == false && bool.TryParse(enableConsoleColors, out bool colors))
        {
            options.EnableConsoleColors = colors;
        }

        Dictionary<LogLevel, ConsoleColor> logLevelColors = 
            fileLogger.GetSection("LogLevelColors").GetChildren().ToDictionary(x =>
            {
                Enum.TryParse(x.Key, out LogLevel level);
                return level;
            }, 
            x =>
            {
                Enum.TryParse(x.Value, out ConsoleColor color);
                return color;
            });

        if (logLevelColors != null)
        {
            options.LogLevelColors = logLevelColors;
        }

        // Override IConfiguration with any provided code-based configuration
        configure?.Invoke(options);

        return new FileLoggerProvider(options);
    }
}
