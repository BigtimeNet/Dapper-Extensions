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
	public class PredicateGroupTests : BasePredicateTest
	{
		ISqlGenerator _generator;

		[SetUp]
		public new void Setup()
		{
			base.Setup();
			_generator = new MockGenerator(Configuration);

		}

		[Test]
		public void EmptyPredicate__CreatesNoOp_And_ReturnsProperSql()
		{
			var predicate = Predicates.Group(GroupOperator.And);
			var parameters = new Dictionary<string, object>();
			var sql = predicate.GetSql(_generator, parameters);

			parameters = new Dictionary<string, object>();
			var sqlSolo = predicate.GetSql(_generator, parameters);
			Assert.IsNotNull(sql);
		}

		[Test]
		public void SinglePredicate__CreatesNoOp_And_ReturnsProperSql()
		{
			var predicate = Predicates.Group(GroupOperator.And);
			var p1 = Predicates.Field<PredicateTestEntity>((x) => x.Name, Operator.Eq, "Test");
			predicate.Predicates.Add(p1);
			var parameters = new Dictionary<string, object>();
			var sql = predicate.GetSql(_generator, parameters);

			parameters = new Dictionary<string, object>();
			var sqlSolo = predicate.GetSql(_generator, parameters);
			Assert.AreEqual($"{sqlSolo}", sql);

		}

		[Test]
		public void GetSql_And_ReturnsProperSql()
		{
			var predicate = Predicates.Group(GroupOperator.And);
			var p1 = Predicates.Field<PredicateTestEntity>((x) => x.Name, Operator.Eq, "Test");
			predicate.Predicates.Add(p1);
			predicate.Predicates.Add(p1);
			var parameters = new Dictionary<string, object>();

			var sql = predicate.GetSql(_generator, parameters);
			parameters = new Dictionary<string, object>();
			var sqlSolo = p1.GetSql(_generator, parameters);
			var sqlSolo2 = p1.GetSql(_generator, parameters);

			Assert.AreEqual($"({sqlSolo} AND {sqlSolo2})", sql);

		}

		[Test]
		public void GetSql_Or_ReturnsProperSql()
		{
			var predicate = Predicates.Group(GroupOperator.Or);
			var p1 = Predicates.Field<PredicateTestEntity>((x) => x.Name, Operator.Eq, "Test");
			predicate.Predicates.Add(p1);
			predicate.Predicates.Add(p1);
			var parameters = new Dictionary<string, object>();

			var sql = predicate.GetSql(_generator, parameters);
			parameters = new Dictionary<string, object>();
			var sqlSolo = p1.GetSql(_generator, parameters);
			var sqlSolo2 = p1.GetSql(_generator, parameters);

			Assert.AreEqual($"({sqlSolo} OR {sqlSolo2})", sql);

		}
	}
}
