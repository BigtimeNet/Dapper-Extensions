using System;
using Microsoft.Extensions.Configuration;

namespace DapperExtensions.Test
{
	public static class TestConfiguration
	{
		public static IConfigurationRoot GetConfiguration()
		{
			var configurationBuilder = new ConfigurationBuilder();
			configurationBuilder
				 .SetBasePath(TestConfiguration.getBasePath())
				 .AddJsonFile("appsettings.json");
			var config = configurationBuilder.Build();
			return config;
		}

		public static string getBasePath()
		{
#if NET451
			return AppDomain.CurrentDomain.BaseDirectory;
#else
			return AppContext.BaseDirectory;
#endif
		}
	}
}
