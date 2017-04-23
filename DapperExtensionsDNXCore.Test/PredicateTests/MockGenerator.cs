using DapperExtensions.Sql;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DapperExtensions;
using DapperExtensions.Mapper;

namespace DapperExtensionsDNXCore.Test.PredicateTests
{
	public class MockGenerator : SqlGeneratorImpl
	{
		public MockGenerator(IDapperExtensionsConfiguration configuration) : base(configuration)
		{
		}
		


	}
}
