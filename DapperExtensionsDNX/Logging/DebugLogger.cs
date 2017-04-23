using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace DapperExtensions.Logging
{
	public class DebugLogger : ILog
	{
		public void Log(string logEntry, Exception ex = null)
		{
			Debug.WriteLine(logEntry);
		}
	}
}
