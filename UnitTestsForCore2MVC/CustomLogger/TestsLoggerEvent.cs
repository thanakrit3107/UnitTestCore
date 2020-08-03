using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Extensions.Logging;

namespace UnitTestsForCore2MVC.CustomLogger
{
    public class TestsLoggerEvent
    {
        public LogLevel LogLevel { get; set; }

        public EventId EventId { get; set; }

        public object State { get; set; }

        public Exception Exception { get; set; }

        public string Message { get; set; }
    }
}
