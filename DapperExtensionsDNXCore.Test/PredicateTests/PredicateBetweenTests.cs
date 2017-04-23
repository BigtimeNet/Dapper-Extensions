using DapperExtensions;
using DapperExtensions.Mapper;
using DapperExtensions.Sql;
using NSubstitute;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DapperExtensionsDNXCore.Test.PredicateTests
{
	[TestFixture]
	public class PredicateBetweenTests : BasePredicateTest
	{
		ISqlGenerator _generator;

		[SetUp]
		public new void Setup()
		{
			base.Setup();
			_generator = new MockGenerator(Configuration);

		}

		[Test]
		public void GetSql_ReturnsProperSql()
		{
			var predicate = Predicates.Between<PredicateTestEntity>((x) => x.Name, new BetweenValues(12, 20), false);
			var parameters = new Dictionary<string, object>();

			var sql = predicate.GetSql(_generator, parameters);

			Assert.AreEqual(2, parameters.Count);
			Assert.AreEqual(12, parameters["@Name_0"]);
			Assert.AreEqual(20, parameters["@Name_1"]);
			Assert.AreEqual("([Table].[Name] BETWEEN @Name_0 AND @Name_1)", sql);
		}

		[Test]
		public void GetSql_Not_ReturnsProperSql()
		{
			var predicate = Predicates.Between<PredicateTestEntity>((x) => x.Name, new BetweenValues(12, 20), true);
			var parameters = new Dictionary<string, object>();

			var sql = predicate.GetSql(_generator, parameters);

			Assert.AreEqual(2, parameters.Count);
			Assert.AreEqual(12, parameters["@Name_0"]);
			Assert.AreEqual(20, parameters["@Name_1"]);
			Assert.AreEqual("([Table].[Name] NOT BETWEEN @Name_0 AND @Name_1)", sql);
		}
	}
}
