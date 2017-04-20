using DapperExtensions;
using DapperExtensions.Mapper;
using NSubstitute;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DapperExtensionsDNXCore.Test.PredicateTests
{
	[TestFixture]
	public class PredicateGenericTests : BasePredicateTest
	{
		[Test]
		public void GetColumnName_GetsColumnName()
		{
			var myPredicate = new MockPredicate();
			var generator = new MockGenerator(Configuration);

			var colNm = myPredicate.GetColumnNameTester(typeof(PredicateTestEntity), generator, "Name");
			Assert.AreEqual("[Table].[Name]", colNm);

		}

		[Test]
		public void GetOperatorString_ReturnsOperatorStrings()
		{
			var myPredicate = new MockPredicateCompare();

			myPredicate.Operator = Operator.Eq;
			myPredicate.Not = false;
			Assert.AreEqual("=", myPredicate.GetOperatorString());
			 
			myPredicate.Operator = Operator.Eq;
			myPredicate.Not = true;
			Assert.AreEqual("<>", myPredicate.GetOperatorString());

			myPredicate.Operator = Operator.Gt;
			myPredicate.Not = false;
			Assert.AreEqual(">", myPredicate.GetOperatorString());
			
			myPredicate.Operator = Operator.Gt;
			myPredicate.Not = true;
			Assert.AreEqual("<=", myPredicate.GetOperatorString());
			
			myPredicate.Operator = Operator.Ge;
			myPredicate.Not = false;
			Assert.AreEqual(">=", myPredicate.GetOperatorString());
			
			myPredicate.Operator = Operator.Lt;
			myPredicate.Not = false;
			Assert.AreEqual("<", myPredicate.GetOperatorString());
			
			myPredicate.Operator = Operator.Lt;
			myPredicate.Not = true;
			Assert.AreEqual(">=", myPredicate.GetOperatorString());
			
			myPredicate.Operator = Operator.Le;
			myPredicate.Not = false;
			Assert.AreEqual("<=", myPredicate.GetOperatorString());
			
			myPredicate.Operator = Operator.Le;
			myPredicate.Not = true;
			Assert.AreEqual(">", myPredicate.GetOperatorString());
			
			myPredicate.Operator = Operator.Like;
			myPredicate.Not = false;
			Assert.AreEqual("LIKE", myPredicate.GetOperatorString());
			
			myPredicate.Operator = Operator.Like;
			myPredicate.Not = true;
			Assert.AreEqual("NOT LIKE", myPredicate.GetOperatorString());

		}
	}
}
