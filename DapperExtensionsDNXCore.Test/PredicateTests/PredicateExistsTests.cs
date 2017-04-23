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
	public class PredicateExistsTests : BasePredicateTest
	{
		ISqlGenerator _generator;

		[SetUp]
		public new void Setup()
		{
			base.Setup();
			_generator = new MockGenerator(Configuration);

		}

		[Test]
		public void PredicateExists_WithoutNot_ReturnsProperSql()
		{
			var pred = Predicates.Field<PredicateTestEntity2>(x=> x.Key,Operator.Eq,22);
			var existsPred = Predicates.Exists<PredicateTestEntity>(pred);

			var parameters = new Dictionary<string, object>();
			var sql = existsPred.GetSql(_generator, parameters);
			parameters = new Dictionary<string, object>();
			var sqlSub = pred.GetSql(_generator, parameters);

			Assert.AreEqual($"(EXISTS (SELECT 1 FROM [Table] WHERE ([Table].[Key] = @Key_0)))", sql);

		}

		[Test]
		public void PredicateExists_WithNot_ReturnsProperSql()
		{
			var pred = Predicates.Field<PredicateTestEntity2>(x => x.Key, Operator.Eq, 22);
			var existsPred = Predicates.Exists<PredicateTestEntity>(pred);
			existsPred.Not = true;

			var parameters = new Dictionary<string, object>();
			var sql = existsPred.GetSql(_generator, parameters);
			parameters = new Dictionary<string, object>();
			var sqlSub = pred.GetSql(_generator, parameters);

			Assert.AreEqual($"(NOT EXISTS (SELECT 1 FROM [Table] WHERE ([Table].[Key] = @Key_0)))", sql);
		}


	}
}
