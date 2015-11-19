using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NLog;
using NoticeSuite.Models;

namespace NoticeSuite.Extensions
{
	public static class LoggerExtension
	{
		public static void LogCallInfo(this Logger logger, string callsid, int patronid, string callstatus, string message)
		{
			LogEventInfo logEvent = new LogEventInfo(LogLevel.Info, logger.Name, message);

			GlobalDiagnosticsContext.Set("callsid", callsid);
			GlobalDiagnosticsContext.Set("patronid", patronid.ToString());
			GlobalDiagnosticsContext.Set("callstatus", callstatus);

			var x = logger.Name;
			logger.Debug(message);			
		}

		public static void LogNotice(this Logger logger, string callsid, int patronid, int itemrecordid, string message)
		{
			LogEventInfo logEvent = new LogEventInfo(LogLevel.Info, logger.Name, message);

			GlobalDiagnosticsContext.Set("callsid", callsid);
			GlobalDiagnosticsContext.Set("patronid", patronid.ToString());
			GlobalDiagnosticsContext.Set("itemrecordid", itemrecordid.ToString());
		
			logger.Debug(message);
		}
	}
}

namespace NoticeSuite
{
	/// <summary>    
	/// Provides methods to write messages with event IDs - useful for the Event Log target.    
	/// Wraps a Logger instance.    
	/// </summary>    
	public class MyLogger
	{
		private Logger _logger;

        public MyLogger(string name)
		{
			_logger = LogManager.GetLogger(name);
		}

        public static MyLogger GetLogger()
		{
			string loggerName;
			Type declaringType;
			int framesToSkip = 1;
			do
			{
				StackFrame frame = new StackFrame(framesToSkip, false);

				var method = frame.GetMethod();
				declaringType = method.DeclaringType;
				if (declaringType == null)
				{
					loggerName = method.Name;
					break;
				}

				framesToSkip++;
				loggerName = declaringType.FullName;
			} while (declaringType.Module.Name.Equals("mscorlib.dll", StringComparison.OrdinalIgnoreCase));

            return new MyLogger(loggerName);
		}

		public void Info(string message, string callsid = null, int? patronid = null, string callstatus = null, int? itemrecordid = null, int? notificationtypeid = null)
		{
			Log(message, LogLevel.Info, callsid, patronid, callstatus, itemrecordid, notificationtypeid, null);
		}

        public void Info(string message)
        {
            Info(message, null, null, null, null);
        }

        public void Info(string message, string callsid = null, int? patronid = null, string barcode = null, Actions action = Actions.None)
        {
            Log(message, LogLevel.Info, callsid, patronid, barcode, null, null, null, (int)action, null);
        }

        public void Debug(string message, string callsid = null, int? patronid = null, string callstatus = null, int? itemrecordid = null, int? notificationtypeid = null)
        {
            Log(message, LogLevel.Debug, callsid, patronid, callstatus, itemrecordid, notificationtypeid, null);
        }

		public void Error(string message, string callsid = null, int? patronid = null, string callstatus = null, int? itemrecordid = null, int? notificationtypeid = null)
		{
			Log(message, LogLevel.Error, callsid, patronid, callstatus, itemrecordid, notificationtypeid, null);
		}

		public void ErrorException(string message, string callsid = null, int? patronid = null, string callstatus = null, int? itemrecordid = null, int? notificationtypeid = null, Exception Exception = null)
		{
			Log(message, LogLevel.Error, callsid, patronid, callstatus, itemrecordid, notificationtypeid, Exception);
		}

        public void Log(string message, LogLevel level, string callsid = null, int? patronid = null, string barcode = null, string callstatus = null, int? itemrecordid = null, int? notificationtypeid = null, int? action = null, Exception ex = null)
        {
            ///            
            /// create log event from the passed message            
            ///             
            LogEventInfo logEvent = new LogEventInfo(level, _logger.Name, message);


            //
            // set event-specific context parameter            
            // this context parameter can be retrieved using ${event-context:EventID}            
            //            
            logEvent.Properties["callsid"] = callsid;
            logEvent.Properties["patronid"] = patronid;
            logEvent.Properties["patronbarcode"] = barcode;
            logEvent.Properties["callstatus"] = callstatus;
            logEvent.Properties["itemrecordid"] = itemrecordid;
            logEvent.Properties["notificationtypeid"] = notificationtypeid;
            logEvent.Properties["actiontaken"] = action;
            logEvent.Exception = ex;
            //             
            // Call the Log() method. It is important to pass typeof(MyLogger) as the            
            // first parameter. If you don't, ${callsite} and other callstack-related             
            // layout renderers will not work properly.            
            //            
            _logger.Log(typeof(MyLogger), logEvent);
        }

		public void Log(string message, LogLevel level, string callsid = null, int? patronid = null, string callstatus = null, int? itemrecordid = null, int? notificationtypeid = null, Exception ex = null)
		{
            Log(message, level, callsid, patronid, null, callstatus, itemrecordid, notificationtypeid, 0, ex);
		}
	}
}