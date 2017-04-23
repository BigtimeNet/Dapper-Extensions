using System;
using System.Collections.Generic;
using System.Linq;
using DapperExtensions.Mapper;
using DapperExtensions.Sql;
using DapperExtensions.Test.Helpers;
using NUnit.Framework;
using NSubstitute;

/*
namespace DapperExtensions.Test
{
	[TestFixture]
	public class PredicatesFixture
	{
		


		[TestFixture]
		public class PropertyPredicateTests : PredicatesFixtureBase
		{
			[Test]
			public void GetSql_ReturnsProperSql()
			{
				var predicate = Setup<PredicateTestEntity, PredicateTestEntity2>("Name", Operator.Eq, "Value", false);
				var parameters = new Dictionary<string, object>();

				var sql = predicate.GetSql(Generator, parameters);

				Assert.AreEqual(0, parameters.Count);
				Assert.AreEqual("(Name = Value)", sql);
			}

			protected PropertyPredicate<T, T2> Setup<T, T2>(string propertyName, Operator op, string propertyName2, bool not)
				 where T : class
				 where T2 : class
			{
				var predicate = Substitute.For<PropertyPredicate<T, T2>>();
				predicate.PropertyName = propertyName;
				predicate.PropertyName2 = propertyName2;
				predicate.Operator = op;
				predicate.Not = not;
				return predicate;
			}
		}

		[TestFixture]
		public class BetweenPredicateTests : PredicatesFixtureBase
		{
			[Test]
			public void GetSql_ReturnsProperSql()
			{
				var predicate = Predicates.Between<PredicateTestEntity>((x)=> x.Name, new BetweenValues(12, 20), false);
				var parameters = new Dictionary<string, object>();

				var sql = predicate.GetSql(Generator, parameters);

				Assert.AreEqual(2, parameters.Count);
				Assert.AreEqual(12, parameters["@Name_0"]);
				Assert.AreEqual(20, parameters["@Name_1"]);
				Assert.AreEqual("(Name BETWEEN @Name_0 AND @Name_1)", sql);
			}

			[Test]
			public void GetSql_Not_ReturnsProperSql()
			{
				var predicate = Predicates.Between<PredicateTestEntity>((x)=> x.Name, new BetweenValues(12, 20), true);
				var parameters = new Dictionary<string, object>();

				var sql = predicate.GetSql(Generator, parameters);

				Assert.AreEqual(2, parameters.Count);
				Assert.AreEqual(12, parameters["@Name_0"]);
				Assert.AreEqual(20, parameters["@Name_1"]);
				Assert.AreEqual("(Name NOT BETWEEN @Name_0 AND @Name_1)", sql);
			}
		}

		[TestFixture]
		public class PredicateGroupTests : PredicatesFixtureBase
		{
			[Test]
			public void EmptyPredicate__CreatesNoOp_And_ReturnsProperSql()
			{
				var predicate = Predicates.Group(GroupOperator.And);
				var p1 = Predicates.Field<PredicateTestEntity>((x) => x.Name, Operator.Eq, "Test");
				predicate.Predicates.Add(p1);
				var parameters = new Dictionary<string, object>();

				var sql = predicate.GetSql(Generator, parameters);

				Assert.AreEqual(0, parameters.Count);
				Assert.AreEqual($"({p1.GetSql(Generator, parameters)})", sql);
			}

			[Test]
			public void GetSql_And_ReturnsProperSql()
			{
				var predicate = Predicates.Group(GroupOperator.And);
				var p1 = Predicates.Field<PredicateTestEntity>((x) => x.Name, Operator.Eq, "Test");
				predicate.Predicates.Add(p1);
				predicate.Predicates.Add(p1);
				var parameters = new Dictionary<string, object>();

				var sql = predicate.GetSql(Generator, parameters);

				Assert.AreEqual(0, parameters.Count);
				Assert.AreEqual($"({p1.GetSql(Generator,parameters)} AND {p1.GetSql(Generator, parameters)})", sql);

			}

			[Test]
			public void GetSql_Or_ReturnsProperSql()
			{
				var predicate = Predicates.Group(GroupOperator.And);
				var p1 = Predicates.Field<PredicateTestEntity>((x) => x.Name, Operator.Eq, "Test");
				predicate.Predicates.Add(p1);
				predicate.Predicates.Add(p1);
				var parameters = new Dictionary<string, object>();

				var sql = predicate.GetSql(Generator, parameters);

				Assert.AreEqual(0, parameters.Count);
				Assert.AreEqual($"({p1.GetSql(Generator, parameters)} OR {p1.GetSql(Generator, parameters)})", sql);

			}

		}

		[TestFixture]
		public class ExistsPredicateTests : PredicatesFixtureBase
		{
			[Test]
			public void GetSql_WithoutNot_ReturnsProperSql()
			{
				var subPredicate = Substitute.For<IPredicate>();
				var subMap = Substitute.For<IClassMapper>();
				var predicate = Predicates.Exists<PredicateTestEntity2>(subPredicate, false);
				Generator.GetTableName(subMap).Returns("subTable");

				var parameters = new Dictionary<string, object>();

				subPredicate.GetSql(Generator, parameters).Returns("subSql");
				var sql = predicate.GetSql(Generator, parameters);

				Assert.AreEqual(0, parameters.Count);
				Assert.AreEqual("(EXISTS (SELECT 1 FROM subTable WHERE subSql))", sql);
			}

			[Test]
			public void GetSql_WithNot_ReturnsProperSql()
			{
				var subPredicate = Substitute.For<IPredicate>();
				var subMap = Substitute.For<IClassMapper>();
				var predicate = Predicates.Exists<PredicateTestEntity2>(subPredicate, true);
				Generator.GetTableName(subMap).Returns("subTable");

				var parameters = new Dictionary<string, object>();

				subPredicate.GetSql(Generator, parameters).Returns("subSql");
				var sql = predicate.GetSql(Generator, parameters);

				Assert.AreEqual(0, parameters.Count);
				Assert.AreEqual("(NOT EXISTS (SELECT 1 FROM subTable WHERE subSql))", sql);
			}

		}
		
	}
}
*/