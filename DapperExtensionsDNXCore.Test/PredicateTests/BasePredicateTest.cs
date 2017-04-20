using DapperExtensions;
using DapperExtensions.Sql;
using NSubstitute;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DapperExtensionsDNXCore.Test.PredicateTests
{
    public abstract class BasePredicateTest
    {
		//protected ISqlDialect SqlDialect;
		//protected ISqlGenerator Generator;
		protected IDapperExtensionsConfiguration Configuration;

		[SetUp]
		public void Setup()
		{

			var dialect = new SqlServerDialect();

			var config = Substitute.For<IDapperExtensionsConfiguration>();
			config.Dialect.Returns<ISqlDialect>(dialect);
			config.GetMap(typeof(PredicateTestEntity)).Returns(new MockClassMapper<PredicateTestEntity>());
			config.GetMap(typeof(PredicateTestEntity2)).Returns(new MockClassMapper<PredicateTestEntity2>());

			Configuration = config;

		}
	}
}
