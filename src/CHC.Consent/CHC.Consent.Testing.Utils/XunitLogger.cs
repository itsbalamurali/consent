﻿using System;
using Microsoft.Extensions.Logging;
using Xunit.Abstractions;

namespace CHC.Consent.Testing.Utils
{
    public class XunitLogger : ILogger
    {
        private readonly ITestOutputHelper _testOutputHelper;
        private readonly string _categoryName;

        public XunitLogger(ITestOutputHelper testOutputHelper, string categoryName)
        {
            _testOutputHelper = testOutputHelper;
            _categoryName = categoryName;
        }

        public IDisposable BeginScope<TState>(TState state) => NoopDisposable.Instance;

        public bool IsEnabled(LogLevel logLevel) => true;

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
        {
            try
            {
                _testOutputHelper.WriteLine($"{_categoryName} [{eventId}] {formatter(state, exception)}");
                if (exception != null)
                    _testOutputHelper.WriteLine(exception.ToString());
            }
            catch (InvalidOperationException)
            {
                
            }
            
        }

        private class NoopDisposable : IDisposable
        {
            public static IDisposable Instance { get; } = new NoopDisposable();
            public void Dispose() { }
        }
    }
    
    public class XunitLogger<T> : XunitLogger, ILogger<T>
    {
        /// <inheritdoc />
        public XunitLogger(ITestOutputHelper testOutputHelper, string categoryName) : base(testOutputHelper, categoryName)
        {
        }
    }
}