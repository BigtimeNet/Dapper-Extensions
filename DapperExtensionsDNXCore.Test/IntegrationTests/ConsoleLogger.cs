using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DapperExtensions;
using System.Diagnostics;

namespace DapperExtensions.Test.IntegrationTests
{
	public class ConsoleLogger : Logging.ILog
	{
		public void Log(string logEntry, Exception ex = null)
		{
			Console.WriteLine(logEntry);
			Debug.WriteLine(logEntry);
		}
	}
}
