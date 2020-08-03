using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;

namespace UnitTestsForCore2MVC.CustomLogger
{
    public class CustomTestsLoggerProvider : ILoggerProvider
    {
        private readonly Func<string, LogLevel, bool> _filter;
        private List<TestsLoggerEvent> _logEventsStore;

        public CustomTestsLoggerProvider(Func<string, LogLevel, bool> filter, List<TestsLoggerEvent> logEventsStore)
        {
            _filter = filter;
            _logEventsStore = logEventsStore;
        }

        public ILogger CreateLogger(string categoryName)
        {
            return new CustomTestsLogger(_logEventsStore, categoryName, _filter);
        }

        public void Dispose()
        {
            //
        }
    }
}
