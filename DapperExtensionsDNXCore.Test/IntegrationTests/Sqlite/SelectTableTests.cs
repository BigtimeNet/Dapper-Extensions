using System;
using System.Collections.Generic;
using System.Linq;
using DapperExtensions.Test.Data;
using NUnit.Framework;


namespace DapperExtensions.Test.IntegrationTests.Sqlite {
	[TestFixture]
	public class SelectTableTests: SqliteBaseFixture {

		[SetUp]
		public void SetupTests() {

			if (Db.Count<Company>(Predicates.Field<Company>(x => x.CompanyNm, Operator.Eq, "ABC")) > 0) { return; }
			var Co1 = new Data.Company { CompanyNm = "Test Company", City = "Chicago", St = "Il", Zip = "60101" };
			var Co2 = new Data.Company { CompanyNm = "ABC", City = "Chicago", St = "Il", Zip = "60101" };
			Db.Insert(Co1);
			Db.Insert(Co2);

			Db.Insert(new Person { Active = true, FirstName = "a", LastName = "a1", DateCreated = DateTime.UtcNow.AddDays(-10), CompanyId = Co1.Id });
			Db.Insert(new Person { Active = false, FirstName = "b", LastName = "b1", DateCreated = DateTime.UtcNow.AddDays(-10), CompanyId = Co1.Id });
			Db.Insert(new Person { Active = true, FirstName = "c", LastName = "c1", DateCreated = DateTime.UtcNow.AddDays(-3), CompanyId = Co2.Id });
			Db.Insert(new Person { Active = false, FirstName = "d", LastName = "d1", DateCreated = DateTime.UtcNow.AddDays(-1), CompanyId = Co2.Id });

		}

		[Test]
		public void SelectTableBasicTest() {

			var personList = Db.GetList<Person>();
			Assert.IsNotNull(personList);
			Assert.IsNotNull(personList.FirstOrDefault(x => x.LastName == "a1" && x.CompanyNm == "Test Company"));
			Assert.IsNotNull(personList.FirstOrDefault(x => x.LastName == "c1" && x.CompanyNm == "ABC"));

			var myId = personList.First().Id;

			var selected = Db.Get<Person>(myId);
			Assert.IsNotNull(selected);
			Assert.IsNotNull(selected.CompanyNm);

		}

		[Test]
		public void SelectTableWithPredicateTest()
		{

			var personList = Db.GetList<Person>(Predicates.Field<Person>(x => x.CompanyNm, Operator.Eq, "ABC"));
			Assert.IsNotNull(personList);
			Assert.AreEqual(2, personList.Count());
			Assert.IsNotNull(personList.FirstOrDefault(x => x.LastName == "c1" && x.CompanyNm == "ABC"));

		}

	}
}
