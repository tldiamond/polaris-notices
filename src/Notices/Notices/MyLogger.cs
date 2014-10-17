using NLog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Notices
{
    class MyLogger
    {
        private Logger _logger;

        public MyLogger(string name)
        {
            _logger = LogManager.GetLogger(name);
        }

        public void WriteMessage(string eventID, string message)
        {
            ///            
            /// create log event from the passed message            
            ///             
            LogEventInfo logEvent = new LogEventInfo(LogLevel.Info, _logger.Name, message);


            //
            // set event-specific context parameter            
            // this context parameter can be retrieved using ${event-context:EventID}            
            //            
            logEvent.Properties["EventID"] = eventID;
            //             
            // Call the Log() method. It is important to pass typeof(MyLogger) as the            
            // first parameter. If you don't, ${callsite} and other callstack-related             
            // layout renderers will not work properly.            
            //            
            _logger.Log(typeof(MyLogger), logEvent);
        }
    }
}