namespace Sitefinity.LibraryItemsDownloader.Tests.Services
{
    using System;
    using System.Collections.Generic;
    using Telerik.Microsoft.Practices.EnterpriseLibrary.Logging;
    using Telerik.Microsoft.Practices.EnterpriseLibrary.Logging.Filters;

    /// <summary>
    /// All Methods are not implemented. It's the reason for the unit tests. 
    /// If some of them is invoked, it will throw an exception meaning that the tests are not configured correctly.
    /// </summary>
    public class TestLogWriter : LogWriter
    {
        public override IDictionary<string, LogSource> TraceSources => throw new NotImplementedException();

        public override T GetFilter<T>()
        {
            throw new NotImplementedException();
        }

        public override T GetFilter<T>(string name)
        {
            throw new NotImplementedException();
        }

        public override ILogFilter GetFilter(string name)
        {
            throw new NotImplementedException();
        }

        public override IEnumerable<LogSource> GetMatchingTraceSources(LogEntry logEntry)
        {
            throw new NotImplementedException();
        }

        public override bool IsLoggingEnabled()
        {
            throw new NotImplementedException();
        }

        public override bool IsTracingEnabled()
        {
            throw new NotImplementedException();
        }

        public override bool ShouldLog(LogEntry log)
        {
            throw new NotImplementedException();
        }

        public override void Write(LogEntry log)
        {
        }
    }
}
