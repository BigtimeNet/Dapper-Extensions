#if NET451
#else
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DapperExtensions.Logging
{
	/// <summary>
	/// If you are using the built-in logging with .Net core, then you can use this implementation of ILog to track the SQL statements being created at runtime.
	/// For simple debug-time logging -- take a look at the ConsoleLogger.
	/// </summary>
	public class MicrosoftLogger : ILog
	{
		private ILogger _log;
		public MicrosoftLogger(ILogger log)
		{
			_log = log;
		}
		public void Log(string logEntry, Exception ex = null)
		{
			if (ex == null)
			{
				_log.LogInformation(logEntry);
			} else
			{
				_log.LogError(new EventId(), ex, logEntry);
			}
		}
	}
}
#endif
