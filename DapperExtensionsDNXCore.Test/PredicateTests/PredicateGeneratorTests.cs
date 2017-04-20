using DapperExtensions;
using NSubstitute;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DapperExtensionsDNXCore.Test.PredicateTests
{
	[TestFixture]
	public class PredicateGeneratorTests 
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
}
