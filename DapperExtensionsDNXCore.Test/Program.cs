using System;
using NUnit.Common;
using NUnitLite;
using System.Reflection;

namespace DapperExtensionsDNXCore.Test
{
    public class Program
    {
        public static int Main(string[] args)
        {
			var result = new AutoRun(typeof(Program).GetTypeInfo().Assembly)
							 .Execute(args, new ExtendedTextWrapper(Console.Out), 
							 Console.In);
			Console.ReadKey();
			return result;
		}
	}
}
