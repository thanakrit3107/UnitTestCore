using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Extensions.Logging;

namespace UnitTestsForCore2MVC.CustomLogger
{
    public class CustomTestsLogger : Microsoft.Extensions.Logging.ILogger
    {
        private List<TestsLoggerEvent> _integrationTestsLoggerStore;
        private string _categoryName;
        private Func<string, LogLevel, bool> _filter;

        public CustomTestsLogger(List<TestsLoggerEvent> store, string categoryName, Func<string, LogLevel, bool> filter)
        {
            _integrationTestsLoggerStore = store;
            _categoryName = categoryName;
            _filter = filter;
        }

        public IDisposable BeginScope<TState>(TState state)
        {
            return null;
        }

        public bool IsEnabled(LogLevel logLevel)
        {
            return (_filter == null || _filter(_categoryName, logLevel));
        }

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
        {
            if(state == null)
            {
                return;
            }
            //var message = formatter(state, exception);

            _integrationTestsLoggerStore.Add(new TestsLoggerEvent()
            {
                LogLevel = logLevel,
                EventId = eventId,
                State = state,
                Message = state.ToString(),
                Exception = exception
            });
        }
    }
}
