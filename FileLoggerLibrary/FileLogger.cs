﻿using Microsoft.Extensions.Logging;

namespace FileLoggerLibrary;

internal class FileLogger : ILogger
{
    private readonly FileLoggerProvider _fileLoggerProvider;
    private readonly string _categoryName;

    private object currentScopeState = null;

    private sealed class Scope : IDisposable
    {
        private FileLogger FileLogger {get;}

        public Scope(FileLogger fileLogger, object state)
        {
            FileLogger = fileLogger;
            FileLogger.currentScopeState = state;
        }

        public void Dispose()
        {
            FileLogger.currentScopeState = null;
        }
    }


    /// <summary>
    /// Default constructor for a FileLogger object.
    /// </summary>
    /// <param name="fileLoggerProvider">The log provider this FileLogger instance is based.</param>
    /// <param name="categoryName">Log or category name for this FileLogger instance.</param>
    /// <exception cref="ArgumentException">Null or empty arguments are not accepted.</exception>
    public FileLogger(FileLoggerProvider fileLoggerProvider, string categoryName)
    {
        _fileLoggerProvider = fileLoggerProvider ?? throw new ArgumentException("Log provider must not be NULL");

        if (string.IsNullOrWhiteSpace(categoryName))
        {
            throw new ArgumentException("Log name must not be NULL or empty");
        }

        _categoryName = categoryName;
    }

    /// <summary>
    /// Checks if the given logLevel is enabled.
    /// </summary>
    /// <param name="logLevel">The log level to check.</param>
    /// <returns></returns>
    public bool IsEnabled(LogLevel logLevel)
    {
        return logLevel >= _fileLoggerProvider.LogMinLevel;
    }

    /// <summary>
    /// Begins a logical operation scope.
    /// </summary>
    /// <typeparam name="TState">Type parameter.</typeparam>
    /// <param name="state">The entry to be written. Can be also an object.</param>
    /// <returns>A disposable object that ends the logical operation scope on dispose.</returns>
    public IDisposable BeginScope<TState>(TState state)
        where TState : notnull
    {
        return new Scope(this, state);
    }

    /// <summary>
    /// Write a log entry.
    /// </summary>
    /// <typeparam name="TState">Type parameter.</typeparam>
    /// <param name="logLevel">Entry will be written on this level.</param>
    /// <param name="eventId">Id of the event.</param>
    /// <param name="state">The entry to be written. Can be also an object.</param>
    /// <param name="exception">The exception related to this entry.</param>
    /// <param name="formatter">Function to create a String message of the state and exception.</param>
    /// <exception cref="ArgumentNullException"></exception>
    public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
    {
        if (IsEnabled(logLevel) == false)
        {
            return;
        }

        ArgumentNullException.ThrowIfNull(nameof(formatter));
        string message = formatter(state, exception);
        _fileLoggerProvider.EnqueueMessage(new LogMessage(message, exception, logLevel, _categoryName, eventId, currentScopeState));
    }
}

