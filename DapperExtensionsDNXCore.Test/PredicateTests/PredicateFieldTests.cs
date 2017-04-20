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
	[TestFixture]
	public class PredicateFieldTests : BasePredicateTest
	{
		ISqlGenerator _generator;

		[SetUp]
		public new void Setup()
		{
			base.Setup();
			_generator = new MockGenerator(Configuration);

		}

		[Test]
		public void GetSql_NullValue_ReturnsProperSql()
		{
			var predicate = new FieldPredicate<PredicateTestEntity>();
			predicate.PropertyName = "Id";
			var parameters = new Dictionary<string, object>();

			var sql = predicate.GetSql(_generator, parameters);

			Assert.AreEqual(0, parameters.Count);
			Assert.AreEqual("([Table].[Id] IS NULL)", sql);
		}

		[Test]
		public void GetSql_NullValue_Not_ReturnsProperSql()
		{
			var predicate = new FieldPredicate<PredicateTestEntity>();
			predicate.PropertyName = "Id";
			predicate.Not = true;
			var parameters = new Dictionary<string, object>();

			var sql = predicate.GetSql(_generator, parameters);

			Assert.AreEqual(0, parameters.Count);
			Assert.AreEqual("([Table].[Id] IS NOT NULL)", sql);
		}

		[Test]
		public void GetSql_NoPropertyReturnsException()
		{
			var predicate = new FieldPredicate<PredicateTestEntity>();
			var parameters = new Dictionary<string, object>();

			var ex = Assert.Throws<NullReferenceException>(() => predicate.GetSql(_generator, parameters));

		}

		[Test]
		public void GetSql_Array_ReturnsProperSql()
		{

			var predicate = new FieldPredicate<PredicateTestEntity>();
			predicate.PropertyName = "Name";
			predicate.Operator =  Operator.Eq;
			predicate.Not = false;
			predicate.Value = new[] { "foo", "bar" };

			var parameters = new Dictionary<string, object>();

			var sql = predicate.GetSql(_generator, parameters);

			Assert.AreEqual(2, parameters.Count);
			Assert.AreEqual("foo", parameters["@Name_0"]);
			Assert.AreEqual("bar", parameters["@Name_1"]);
			Assert.AreEqual("([Table].[Name] IN (@Name_0, @Name_1))", sql);

		}

		[Test]
		public void GetSql_Array_Not_ReturnsProperSql()
		{
			var predicate = new FieldPredicate<PredicateTestEntity>();
			predicate.PropertyName = "Name";
			predicate.Operator = Operator.Eq;
			predicate.Not = true;
			predicate.Value = new[] { "foo", "bar" };

			var parameters = new Dictionary<string, object>();

			var sql = predicate.GetSql(_generator, parameters);

			Assert.AreEqual(2, parameters.Count);
			Assert.AreEqual("foo", parameters["@Name_0"]);
			Assert.AreEqual("bar", parameters["@Name_1"]);
			Assert.AreEqual("([Table].[Name] NOT IN (@Name_0, @Name_1))", sql);

		}

		[Test]
		public void GetSql_ReturnsProperSql()
		{
			var predicate = Setup<PredicateTestEntity>("Name", Operator.Eq, 12, false);
			var parameters = new Dictionary<string, object>();

			var sql = predicate.GetSql(_generator, parameters);

			Assert.AreEqual(1, parameters.Count);
			Assert.AreEqual(12, parameters["@Name_0"]);
			Assert.AreEqual("([Table].[Name] = @Name_0)", sql);
		}

		protected FieldPredicate<T> Setup<T>(string propertyName, Operator op, object value, bool not) where T : class
		{
			var predicate = new FieldPredicate<T>();
			predicate.PropertyName = propertyName;
			predicate.Operator = op;
			predicate.Not = not;
			predicate.Value = value;
			return predicate;
		}
	}

}
