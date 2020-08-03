using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;

namespace UnitTestsForCore2MVC.CustomLogger
{
    public static class TestsLoggerExtensions
    {
        public static ILoggerFactory AddTestsLogger(this ILoggerFactory factory, List<TestsLoggerEvent> logEventsStore,
                                        Func<string, LogLevel, bool> filter = null)
        {
            factory.AddProvider(new CustomTestsLoggerProvider(filter, logEventsStore));
            return factory;
        }
    }
}
