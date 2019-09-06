using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using Dapper;
using DapperExtensions.Mapper;
using DapperExtensions.Sql;
using NUnit.Framework;

namespace DapperExtensions.Test.IntegrationTests.SqlServer
{
	public class SqlServerBaseFixture
	{
		protected IDatabase Db;

		[SetUp]
		public virtual void Setup()
		{
			var connectString = "";
#if NET451
            connectString = TestConfiguration.GetConnectionString();
#else
            var connectStringSetting = TestConfiguration.GetConfiguration().GetSection("ConnectionStrings").GetChildren().FirstOrDefault(x => x.Key == "SQLServer");
            if (connectStringSetting != null) { connectString = connectStringSetting.Value; }
#endif
            if (connectString =="") {
				Assert.Inconclusive("There is no SQL server connection string in appsettings.json for this test project.");
				return;
			}

			var connection = new SqlConnection(connectString);
			var config = new DapperExtensionsConfiguration(typeof(AutoClassMapper<>), new List<Assembly>(), new SqlServerDialect());
			var sqlGenerator = new SqlGeneratorImpl(config);
			Db = new Database(connection, sqlGenerator, new Logging.DebugLogger());
			var files = new List<string>
										  {
												ReadScriptFile("CreateAnimalTable"),
												ReadScriptFile("CreateFooTable"),
												ReadScriptFile("CreateMultikeyTable"),
												ReadScriptFile("CreateNullableTestClassTable"),
												ReadScriptFile("CreatePersonTable"),
                                                ReadScriptFile("CreateCarTable"),
                                                ReadScriptFile("CreateCityTable"),
                                                ReadScriptFile("CreateCompanyTable"),
												ReadScriptFile("CreatePersonView")
										  };

			foreach (var setupFile in files)
			{
				connection.Execute(setupFile);
			}
		}

		public string ReadScriptFile(string name)
		{
			var SQLPath = System.IO.Path.Combine(TestConfiguration.getBasePath(), @"SQLFiles\SQLServer");
			string fileName = System.IO.Path.Combine(SQLPath, name + ".sql");
			return System.IO.File.ReadAllText(fileName);
		}

	}
}