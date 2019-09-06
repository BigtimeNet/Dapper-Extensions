using System;
using System.Collections.Generic;
using System.IO;
#if NET451
using Newtonsoft.Json;
#else
using Microsoft.Extensions.Configuration;
#endif

namespace DapperExtensions.Test
{
	public static class TestConfiguration
	{
#if NET451
        private class AppSettings {
            public Dictionary<string, string> ConnectionStrings { get; set; }
        }

        public static string GetConnectionString() {
            var appSettings = System.IO.Path.Combine(TestConfiguration.getBasePath(), @"appsettings.json");
            var settings = JsonConvert.DeserializeObject<AppSettings>(File.ReadAllText(appSettings));
            return settings.ConnectionStrings["SQLServer"];
        }
#else
        public static Microsoft.Extensions.Configuration.IConfigurationRoot GetConfiguration() {
            var configurationBuilder = new Microsoft.Extensions.Configuration.ConfigurationBuilder();
            configurationBuilder
                 .SetBasePath(TestConfiguration.getBasePath())
                 .AddJsonFile("appsettings.json");
            var config = configurationBuilder.Build();
            return config;
        }
#endif
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
