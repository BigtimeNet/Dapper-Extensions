using System;
using System.Collections.Generic;
using System.Linq;
using DapperExtensions.Mapper;
using DapperExtensions.Sql;
using DapperExtensions.Test.Helpers;
using NUnit.Framework;
using NSubstitute;

namespace DapperExtensions.Test
{
	[TestFixture]
	public class PredicatesFixture
	{
		public abstract class PredicatesFixtureBase
		{
			protected ISqlDialect SqlDialect;
			protected ISqlGenerator Generator;
			protected IDapperExtensionsConfiguration Configuration;

			[SetUp]
			public void Setup()
			{

				var dialect = Substitute.For<ISqlDialect>();
				dialect.ParameterPrefix.Returns<char>('@');
				SqlDialect = dialect;

				var config = Substitute.For<IDapperExtensionsConfiguration>();
				config.Dialect.Returns<ISqlDialect>(SqlDialect);
				Configuration = config;

				var gen = Substitute.For<ISqlGenerator>();
				gen.Configuration.Returns<IDapperExtensionsConfiguration>(Configuration);
				Generator = gen;

			}
		}

		[TestFixture]
		public class PredicatesTests : PredicatesFixtureBase
		{
			[Test]
			public void Field_ReturnsSetupPredicate()
			{
				var predicate = Predicates.Field<PredicateTestEntity>(f => f.Name, Operator.Like, "Lead", true);
				Assert.AreEqual("Name", predicate.PropertyName);
				Assert.AreEqual(Operator.Like, predicate.Operator);
				Assert.AreEqual("Lead", predicate.Value);
				Assert.AreEqual(true, predicate.Not);
			}

			[Test]
			public void Property_ReturnsSetupPredicate()
			{
				var predicate = Predicates.Property<PredicateTestEntity, PredicateTestEntity2>(f => f.Name, Operator.Le, f => f.Value, true);
				Assert.AreEqual("Name", predicate.PropertyName);
				Assert.AreEqual(Operator.Le, predicate.Operator);
				Assert.AreEqual("Value", predicate.PropertyName2);
				Assert.AreEqual(true, predicate.Not);
			}

			[Test]
			public void Group_ReturnsSetupPredicate()
			{
				IPredicate subPredicate = Substitute.For<IPredicate>();
				var predicate = Predicates.Group(GroupOperator.Or, subPredicate);
				Assert.AreEqual(GroupOperator.Or, predicate.Operator);
				Assert.AreEqual(1, predicate.Predicates.Count);
				Assert.AreEqual(subPredicate, predicate.Predicates[0]);
			}

			[Test]
			public void Exists_ReturnsSetupPredicate()
			{
				IPredicate subPredicate = Substitute.For<IPredicate>();
				var predicate = Predicates.Exists<PredicateTestEntity2>(subPredicate, true);
				Assert.AreEqual(subPredicate, predicate.Predicate);
				Assert.AreEqual(true, predicate.Not);
			}

			[Test]
			public void Between_ReturnsSetupPredicate()
			{
				BetweenValues values = new BetweenValues();
				var predicate = Predicates.Between<PredicateTestEntity>(f => f.Name, values, true);
				Assert.AreEqual("Name", predicate.PropertyName);
				Assert.AreEqual(values, predicate.Value);
				Assert.AreEqual(true, predicate.Not);
			}

			[Test]
			public void Sort__ReturnsSetupPredicate()
			{
				var predicate = Predicates.Sort<PredicateTestEntity>(f => f.Name, false);
				Assert.AreEqual("Name", predicate.PropertyName);
				Assert.AreEqual(false, predicate.Ascending);
			}
		}

		[TestFixture]
		public class BasePredicateTests : PredicatesFixtureBase
		{

			[Test]
			public void GetColumnName_GetsColumnName()
			{
				var classMapper = Substitute.For<IClassMapper>();
				var predicate = Substitute.For<BasePredicate>();
				var propertyMap = Substitute.For<IPropertyMap>();
				List<IPropertyMap> propertyMaps = new List<IPropertyMap> { propertyMap };
				//predicate.CallBase = true;

				Configuration.GetMap(typeof(PredicateTestEntity)).Returns(classMapper);
				classMapper.Properties.Returns(propertyMaps);
				propertyMap.Name.Returns("Name");
				Generator.GetColumnName(classMapper, propertyMap, false).Returns("foo");

				var result = predicate.TestProtected().RunMethod<string>("GetColumnName", typeof(PredicateTestEntity), Generator, "Name");

				Configuration.Received();
				classMapper.Received();
				propertyMap.Received();
				Generator.Received();

				StringAssert.StartsWith("foo", result);

			}
		}

		[TestFixture]
		public class ComparePredicateTests : PredicatesFixtureBase
		{
			[Test]
			public void GetOperatorString_ReturnsOperatorStrings()
			{
				Assert.AreEqual("=", Setup(Operator.Eq, false).GetOperatorString());
				Assert.AreEqual("<>", Setup(Operator.Eq, true).GetOperatorString());
				Assert.AreEqual(">", Setup(Operator.Gt, false).GetOperatorString());
				Assert.AreEqual("<=", Setup(Operator.Gt, true).GetOperatorString());
				Assert.AreEqual(">=", Setup(Operator.Ge, false).GetOperatorString());
				Assert.AreEqual("<", Setup(Operator.Ge, true).GetOperatorString());
				Assert.AreEqual("<", Setup(Operator.Lt, false).GetOperatorString());
				Assert.AreEqual(">=", Setup(Operator.Lt, true).GetOperatorString());
				Assert.AreEqual("<=", Setup(Operator.Le, false).GetOperatorString());
				Assert.AreEqual(">", Setup(Operator.Le, true).GetOperatorString());
				Assert.AreEqual("LIKE", Setup(Operator.Like, false).GetOperatorString());
				Assert.AreEqual("NOT LIKE", Setup(Operator.Like, true).GetOperatorString());
			}

			protected ComparePredicate Setup(Operator op, bool not)
			{
				var predicate = Substitute.For<ComparePredicate>();
				predicate.Operator = op;
				predicate.Not = not;
				return predicate;
			}
		}

		[TestFixture]
		public class FieldPredicateTests : PredicatesFixtureBase
		{
			[Test]
			public void GetSql_NullValue_ReturnsProperSql()
			{
				var predicate = Setup<PredicateTestEntity>("Name", Operator.Eq, null, false);
				var parameters = new Dictionary<string, object>();

				var sql = predicate.GetSql(Generator, parameters);

				Assert.AreEqual(0, parameters.Count);
				Assert.AreEqual("(fooCol IS NULL)", sql);
			}

			[Test]
			public void GetSql_NullValue_Not_ReturnsProperSql()
			{
				var predicate = Setup<PredicateTestEntity>("Name", Operator.Eq, null, true);
				var parameters = new Dictionary<string, object>();

				var sql = predicate.GetSql(Generator, parameters);

				Assert.AreEqual(0, parameters.Count);
				Assert.AreEqual("(fooCol IS NOT NULL)", sql);
			}

			[Test]
			public void GetSql_Enumerable_NotEqOperator_ReturnsProperSql()
			{
				var predicate = Setup<PredicateTestEntity>("Name", Operator.Le, new[] { "foo", "bar" }, false);
				var parameters = new Dictionary<string, object>();

				var ex = Assert.Throws<ArgumentException>(() => predicate.GetSql(Generator, parameters));

				StringAssert.StartsWith("Operator must be set to Eq for Enumerable types", ex.Message);
			}

			[Test]
			public void GetSql_Enumerable_ReturnsProperSql()
			{
				var predicate = Setup<PredicateTestEntity>("Name", Operator.Eq, new[] { "foo", "bar" }, false);
				var parameters = new Dictionary<string, object>();

				var sql = predicate.GetSql(Generator, parameters);

				Assert.AreEqual(2, parameters.Count);
				Assert.AreEqual("foo", parameters["@Name_0"]);
				Assert.AreEqual("bar", parameters["@Name_1"]);
				Assert.AreEqual("(fooCol IN (@Name_0, @Name_1))", sql);
			}

			[Test]
			public void GetSql_Enumerable_Not_ReturnsProperSql()
			{
				var predicate = Setup<PredicateTestEntity>("Name", Operator.Eq, new[] { "foo", "bar" }, true);
				var parameters = new Dictionary<string, object>();

				var sql = predicate.GetSql(Generator, parameters);

				Assert.AreEqual(2, parameters.Count);
				Assert.AreEqual("foo", parameters["@Name_0"]);
				Assert.AreEqual("bar", parameters["@Name_1"]);
				Assert.AreEqual("(fooCol NOT IN (@Name_0, @Name_1))", sql);
			}

			[Test]
			public void GetSql_ReturnsProperSql()
			{
				var predicate = Setup<PredicateTestEntity>("Name", Operator.Eq, 12, true);
				var parameters = new Dictionary<string, object>();

				var sql = predicate.GetSql(Generator, parameters);

				Assert.AreEqual(1, parameters.Count);
				Assert.AreEqual(12, parameters["@Name_0"]);
				Assert.AreEqual("(fooCol = @Name_0)", sql);
			}

			protected FieldPredicate<T> Setup<T>(string propertyName, Operator op, object value, bool not) where T : class
			{
				var predicate = Substitute.For<FieldPredicate<T>>();
				predicate.PropertyName = propertyName;
				predicate.Operator = op;
				predicate.Not = not;
				predicate.Value = value;
				return predicate;
			}
		}

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

		public class PredicateTestEntity
		{
			public int Id { get; set; }
			public string Name { get; set; }
		}

		public class PredicateTestEntity2
		{
			public int Key { get; set; }
			public string Value { get; set; }
		}
	}
}