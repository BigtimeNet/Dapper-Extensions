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
	public class PredicatePropertyTests : BasePredicateTest
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
			var predicate = Setup<PredicateTestEntity, PredicateTestEntity2>("Name", Operator.Eq, "Value", false);
			var parameters = new Dictionary<string, object>();

			var sql = predicate.GetSql(_generator, parameters);

			Assert.AreEqual(0, parameters.Count);
			Assert.AreEqual("([Table].[Name] = [Table].[Value])", sql);
		}

		protected PropertyPredicate<T, T2> Setup<T, T2>(string propertyName, Operator op, string propertyName2, bool not)
			 where T : class
			 where T2 : class
		{
			var predicate = new PropertyPredicate<T, T2>();
			predicate.PropertyName = propertyName;
			predicate.PropertyName2 = propertyName2;
			predicate.Operator = op;
			predicate.Not = not;
			return predicate;
		}
	}
}
