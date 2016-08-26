using System.Text;
using System;
using System.Collections.Generic;
using System.Linq;
using DapperExtensions.Test.Data;
using NUnit.Framework;
using System.Threading.Tasks;

namespace DapperExtensions.Test.IntegrationTests.Sqlite {
	[TestFixture]
	public class CrudAsyncFixture {
		[TestFixture]
		public class InsertMethod : SqliteBaseFixture {
			[Test]
			public async Task AddsEntityToDatabase_ReturnsKey() {
				Person p = new Person { Active = true, FirstName = "Foo", LastName = "Bar", DateCreated = DateTime.UtcNow };
				int id = (int)(await Db.InsertAsync(p));
				Assert.AreEqual(1, id);
				Assert.AreEqual(1, p.Id);
			}

			//[Test] //TODO: multikey identity
			public async Task AddsEntityToDatabase_ReturnsCompositeKey() {
				Multikey m = new Multikey { Key2 = "key", Value = "foo" };
				var key = await Db.InsertAsync(m);
				Assert.AreEqual(1, key.Key1);
				Assert.AreEqual("key", key.Key2);
			}

			[Test]
			public async Task AddsEntityToDatabase_ReturnsGeneratedPrimaryKey() {
				Person p1 = new Person {
					Active = true,
					FirstName = "Foo",
					LastName = "Bar",
					DateCreated = DateTime.UtcNow
				};
				await Db.InsertAsync(p1);

				Person p2 = await Db.GetAsync<Person>(p1.Id);

				Assert.AreNotEqual(0, p2.Id);
				Assert.AreEqual(p1.Id, p2.Id);

				Animal a1 = new Animal { Name = "Foo" };
				await Db.InsertAsync(a1);
				Animal a2 = Db.Get<Animal>(a1.Id);
				Assert.AreNotEqual(0, a2.Id);
				Assert.AreEqual(a1.Id, a2.Id);

			}

			[Test]
			public async Task AddsMultipleEntitiesToDatabase() {
				Animal a1 = new Animal { Name = "Foo" };
				Animal a2 = new Animal { Name = "Bar" };
				Animal a3 = new Animal { Name = "Baz" };

				await Db.InsertAsync<Animal>(new[] { a1, a2, a3 });

				var animals = (await Db.GetListAsync<Animal>()).ToList();
				Assert.AreEqual(3, animals.Count);
			}
		}

		[TestFixture]
		public class GetMethod : SqliteBaseFixture {
			[Test]
			public async Task UsingKey_ReturnsEntity() {
				Person p1 = new Person {
					Active = true,
					FirstName = "Foo",
					LastName = "Bar",
					DateCreated = DateTime.UtcNow
				};
				int id = (int)(await Db.InsertAsync(p1));

				Person p2 = await Db.GetAsync<Person>(id);
				Assert.AreEqual(id, p2.Id);
				Assert.AreEqual("Foo", p2.FirstName);
				Assert.AreEqual("Bar", p2.LastName);
			}

			//[Test] TODO: multikey identity
			public async Task UsingCompositeKey_ReturnsEntity() {
				Multikey m1 = new Multikey { Key2 = "key", Value = "bar" };
				var key = await Db.InsertAsync(m1);

				Multikey m2 = await Db.GetAsync<Multikey>(new { key.Key1, key.Key2 });
				Assert.AreEqual(1, m2.Key1);
				Assert.AreEqual("key", m2.Key2);
				Assert.AreEqual("bar", m2.Value);
			}
		}

		[TestFixture]
		public class DeleteMethod : SqliteBaseFixture {
			[Test]
			public async Task UsingKey_DeletesFromDatabase() {
				Person p1 = new Person {
					Active = true,
					FirstName = "Foo",
					LastName = "Bar",
					DateCreated = DateTime.UtcNow
				};
				int id = (int)(await Db.InsertAsync(p1));

				Person p2 = await Db.GetAsync<Person>(id);
				await Db.DeleteAsync(p2);
				Assert.IsNull(await Db.GetAsync<Person>(id));
			}

			//[Test] TODO: multikey identity
			public async Task UsingCompositeKey_DeletesFromDatabase() {
				Multikey m1 = new Multikey { Key2 = "key", Value = "bar" };
				var key = await Db.InsertAsync(m1);

				Multikey m2 = await Db.GetAsync<Multikey>(new { key.Key1, key.Key2 });
				await Db.DeleteAsync(m2);
				Assert.IsNull(await Db.GetAsync<Multikey>(new { key.Key1, key.Key2 }));
			}

			[Test]
			public async Task UsingPredicate_DeletesRows() {
				Person p1 = new Person { Active = true, FirstName = "Foo", LastName = "Bar", DateCreated = DateTime.UtcNow };
				Person p2 = new Person { Active = true, FirstName = "Foo", LastName = "Bar", DateCreated = DateTime.UtcNow };
				Person p3 = new Person { Active = true, FirstName = "Foo", LastName = "Barz", DateCreated = DateTime.UtcNow };
				await Db.InsertAsync(p1);
				await Db.InsertAsync(p2);
				await Db.InsertAsync(p3);

				var list = await Db.GetListAsync<Person>();
				Assert.AreEqual(3, list.Count());

				IPredicate pred = Predicates.Field<Person>(p => p.LastName, Operator.Eq, "Bar");
				var result = await Db.DeleteAsync<Person>(pred);
				Assert.IsTrue(result);

				list = await Db.GetListAsync<Person>();
				Assert.AreEqual(1, list.Count());
			}

			[Test]
			public async Task UsingObject_DeletesRows() {
				Person p1 = new Person { Active = true, FirstName = "Foo", LastName = "Bar", DateCreated = DateTime.UtcNow };
				Person p2 = new Person { Active = true, FirstName = "Foo", LastName = "Bar", DateCreated = DateTime.UtcNow };
				Person p3 = new Person { Active = true, FirstName = "Foo", LastName = "Barz", DateCreated = DateTime.UtcNow };
				await Db.InsertAsync(p1);
				await Db.InsertAsync(p2);
				await Db.InsertAsync(p3);

				var list = await Db.GetListAsync<Person>();
				Assert.AreEqual(3, list.Count());

				var result = await Db.DeleteAsync<Person>(new { LastName = "Bar" });
				Assert.IsTrue(result);

				list = await Db.GetListAsync<Person>();
				Assert.AreEqual(1, list.Count());
			}
		}

		[TestFixture]
		public class UpdateMethod : SqliteBaseFixture {
			[Test]
			public async Task UsingKey_UpdatesEntity() {
				Person p1 = new Person {
					Active = true,
					FirstName = "Foo",
					LastName = "Bar",
					DateCreated = DateTime.UtcNow
				};
				int id = (int)(await Db.InsertAsync(p1));

				var p2 = await Db.GetAsync<Person>(id);
				p2.FirstName = "Baz";
				p2.Active = false;

				await Db.UpdateAsync(p2);

				var p3 = await Db.GetAsync<Person>(id);
				Assert.AreEqual("Baz", p3.FirstName);
				Assert.AreEqual("Bar", p3.LastName);
				Assert.AreEqual(false, p3.Active);
			}

			[Test]
			[Ignore("SQLLite doesnt support a composite key with both an IDENTITY column and a user-generated value.")]
			public async Task UsingCompositeKey_UpdatesEntity() {

				Multikey m1 = new Multikey { Key1 = 1, Key2 = "key", Value = "bar" };
				var key = await Db.InsertAsync(m1);

				Multikey m2 = await Db.GetAsync<Multikey>(new { key.Key1, key.Key2 });
				m2.Key2 = "key";
				m2.Value = "barz";
				await Db.UpdateAsync(m2);

				Multikey m3 = await Db.GetAsync<Multikey>(new { Key1 = 2, Key2 = "key" });
				Assert.AreEqual(1, m3.Key1);
				Assert.AreEqual("key", m3.Key2);

			}
		}

		[TestFixture]
		public class GetListMethod : SqliteBaseFixture {
			[Test]
			public async Task UsingNullPredicate_ReturnsAll() {
				await Db.InsertAsync(new Person { Active = true, FirstName = "a", LastName = "a1", DateCreated = DateTime.UtcNow });
				await Db.InsertAsync(new Person { Active = false, FirstName = "b", LastName = "b1", DateCreated = DateTime.UtcNow });
				await Db.InsertAsync(new Person { Active = true, FirstName = "c", LastName = "c1", DateCreated = DateTime.UtcNow });
				await Db.InsertAsync(new Person { Active = false, FirstName = "d", LastName = "d1", DateCreated = DateTime.UtcNow });

				IEnumerable<Person> list = await Db.GetListAsync<Person>();
				Assert.AreEqual(4, list.Count());
			}

			[Test]
			public async Task UsingPredicate_ReturnsMatching() {
				await Db.InsertAsync(new Person { Active = true, FirstName = "a", LastName = "a1", DateCreated = DateTime.UtcNow });
				await Db.InsertAsync(new Person { Active = false, FirstName = "b", LastName = "b1", DateCreated = DateTime.UtcNow });
				await Db.InsertAsync(new Person { Active = true, FirstName = "c", LastName = "c1", DateCreated = DateTime.UtcNow });
				await Db.InsertAsync(new Person { Active = false, FirstName = "d", LastName = "d1", DateCreated = DateTime.UtcNow });

				var predicate = Predicates.Field<Person>(f => f.Active, Operator.Eq, true);
				IEnumerable<Person> list = await Db.GetListAsync<Person>(predicate, null);
				Assert.AreEqual(2, list.Count());
				Assert.IsTrue(list.All(p => p.FirstName == "a" || p.FirstName == "c"));
			}

			[Test]
			public async Task UsingObject_ReturnsMatching() {
				await Db.InsertAsync(new Person { Active = true, FirstName = "a", LastName = "a1", DateCreated = DateTime.UtcNow });
				await Db.InsertAsync(new Person { Active = false, FirstName = "b", LastName = "b1", DateCreated = DateTime.UtcNow });
				await Db.InsertAsync(new Person { Active = true, FirstName = "c", LastName = "c1", DateCreated = DateTime.UtcNow });
				await Db.InsertAsync(new Person { Active = false, FirstName = "d", LastName = "d1", DateCreated = DateTime.UtcNow });

				var predicate = new { Active = true, FirstName = "c" };
				IEnumerable<Person> list = await Db.GetListAsync<Person>(predicate, null);
				Assert.AreEqual(1, list.Count());
				Assert.IsTrue(list.All(p => p.FirstName == "c"));
			}
		}

		[TestFixture]
		public class GetPageMethod : SqliteBaseFixture {
			[Test]
			public async Task UsingNullPredicate_ReturnsMatching() {
				var id1 = await Db.InsertAsync(new Person { Active = true, FirstName = "Sigma", LastName = "Alpha", DateCreated = DateTime.UtcNow });
				var id2 = await Db.InsertAsync(new Person { Active = false, FirstName = "Delta", LastName = "Alpha", DateCreated = DateTime.UtcNow });
				var id3 = await Db.InsertAsync(new Person { Active = true, FirstName = "Theta", LastName = "Gamma", DateCreated = DateTime.UtcNow });
				var id4 = await Db.InsertAsync(new Person { Active = false, FirstName = "Iota", LastName = "Beta", DateCreated = DateTime.UtcNow });

				IList<ISort> sort = new List<ISort>
										  {
													 Predicates.Sort<Person>(p => p.LastName),
													 Predicates.Sort<Person>(p => p.FirstName)
												};

				IEnumerable<Person> list = await Db.GetPageAsync<Person>(null, sort, 0, 2);
				Assert.AreEqual(2, list.Count());
				Assert.AreEqual(id2, list.First().Id);
				Assert.AreEqual(id1, list.Skip(1).First().Id);
			}

			[Test]
			public async Task UsingPredicate_ReturnsMatching() {
				var id1 = await Db.InsertAsync(new Person { Active = true, FirstName = "Sigma", LastName = "Alpha", DateCreated = DateTime.UtcNow });
				var id2 = await Db.InsertAsync(new Person { Active = false, FirstName = "Delta", LastName = "Alpha", DateCreated = DateTime.UtcNow });
				var id3 = await Db.InsertAsync(new Person { Active = true, FirstName = "Theta", LastName = "Gamma", DateCreated = DateTime.UtcNow });
				var id4 = await Db.InsertAsync(new Person { Active = false, FirstName = "Iota", LastName = "Beta", DateCreated = DateTime.UtcNow });

				var predicate = Predicates.Field<Person>(f => f.Active, Operator.Eq, true);
				IList<ISort> sort = new List<ISort>
										  {
													 Predicates.Sort<Person>(p => p.LastName),
													 Predicates.Sort<Person>(p => p.FirstName)
												};

				IEnumerable<Person> list = await Db.GetPageAsync<Person>(predicate, sort, 0, 3);
				Assert.AreEqual(2, list.Count());
				Assert.IsTrue(list.All(p => p.FirstName == "Sigma" || p.FirstName == "Theta"));
			}

			[Test]
			public async Task NotFirstPage_Returns_NextResults() {
				var id1 = await Db.InsertAsync(new Person { Active = true, FirstName = "Sigma", LastName = "Alpha", DateCreated = DateTime.UtcNow });
				var id2 = await Db.InsertAsync(new Person { Active = false, FirstName = "Delta", LastName = "Alpha", DateCreated = DateTime.UtcNow });
				var id3 = await Db.InsertAsync(new Person { Active = true, FirstName = "Theta", LastName = "Gamma", DateCreated = DateTime.UtcNow });
				var id4 = await Db.InsertAsync(new Person { Active = false, FirstName = "Iota", LastName = "Beta", DateCreated = DateTime.UtcNow });

				IList<ISort> sort = new List<ISort>
										  {
													 Predicates.Sort<Person>(p => p.LastName),
													 Predicates.Sort<Person>(p => p.FirstName)
												};

				IEnumerable<Person> list = await Db.GetPageAsync<Person>(null, sort, 1, 2);
				Assert.AreEqual(2, list.Count());
				Assert.AreEqual(id4, list.First().Id);
				Assert.AreEqual(id3, list.Skip(1).First().Id);
			}

			[Test]
			public async Task UsingObject_ReturnsMatching() {
				var id1 = await Db.InsertAsync(new Person { Active = true, FirstName = "Sigma", LastName = "Alpha", DateCreated = DateTime.UtcNow });
				var id2 = await Db.InsertAsync(new Person { Active = false, FirstName = "Delta", LastName = "Alpha", DateCreated = DateTime.UtcNow });
				var id3 = await Db.InsertAsync(new Person { Active = true, FirstName = "Theta", LastName = "Gamma", DateCreated = DateTime.UtcNow });
				var id4 = await Db.InsertAsync(new Person { Active = false, FirstName = "Iota", LastName = "Beta", DateCreated = DateTime.UtcNow });

				var predicate = new { Active = true };
				IList<ISort> sort = new List<ISort>
										  {
													 Predicates.Sort<Person>(p => p.LastName),
													 Predicates.Sort<Person>(p => p.FirstName)
												};

				IEnumerable<Person> list = await Db.GetPageAsync<Person>(predicate, sort, 0, 3);
				Assert.AreEqual(2, list.Count());
				Assert.IsTrue(list.All(p => p.FirstName == "Sigma" || p.FirstName == "Theta"));
			}
		}

		[TestFixture]
		public class CountMethod : SqliteBaseFixture {
			[Test]
			public async Task UsingNullPredicate_Returns_Count() {
				await Db.InsertAsync(new Person { Active = true, FirstName = "a", LastName = "a1", DateCreated = DateTime.UtcNow.AddDays(-10) });
				await Db.InsertAsync(new Person { Active = false, FirstName = "b", LastName = "b1", DateCreated = DateTime.UtcNow.AddDays(-10) });
				await Db.InsertAsync(new Person { Active = true, FirstName = "c", LastName = "c1", DateCreated = DateTime.UtcNow.AddDays(-3) });
				await Db.InsertAsync(new Person { Active = false, FirstName = "d", LastName = "d1", DateCreated = DateTime.UtcNow.AddDays(-1) });

				int count = await Db.CountAsync<Person>(null);
				Assert.AreEqual(4, count);
			}

			[Test]
			public async Task UsingPredicate_Returns_Count() {
				await Db.InsertAsync(new Person { Active = true, FirstName = "a", LastName = "a1", DateCreated = DateTime.UtcNow.AddDays(-10) });
				await Db.InsertAsync(new Person { Active = false, FirstName = "b", LastName = "b1", DateCreated = DateTime.UtcNow.AddDays(-10) });
				await Db.InsertAsync(new Person { Active = true, FirstName = "c", LastName = "c1", DateCreated = DateTime.UtcNow.AddDays(-3) });
				await Db.InsertAsync(new Person { Active = false, FirstName = "d", LastName = "d1", DateCreated = DateTime.UtcNow.AddDays(-1) });

				var predicate = Predicates.Field<Person>(f => f.DateCreated, Operator.Lt, DateTime.UtcNow.AddDays(-5));
				int count = await Db.CountAsync<Person>(predicate);
				Assert.AreEqual(2, count);
			}

			[Test]
			public async Task UsingObject_Returns_Count() {
				await Db.InsertAsync(new Person { Active = true, FirstName = "a", LastName = "a1", DateCreated = DateTime.UtcNow.AddDays(-10) });
				await Db.InsertAsync(new Person { Active = false, FirstName = "b", LastName = "b1", DateCreated = DateTime.UtcNow.AddDays(-10) });
				await Db.InsertAsync(new Person { Active = true, FirstName = "c", LastName = "c1", DateCreated = DateTime.UtcNow.AddDays(-3) });
				await Db.InsertAsync(new Person { Active = false, FirstName = "d", LastName = "d1", DateCreated = DateTime.UtcNow.AddDays(-1) });

				var predicate = new { FirstName = new[] { "b", "d" } };
				int count = await Db.CountAsync<Person>(predicate);
				Assert.AreEqual(2, count);
			}
		}

		[TestFixture]
		public class GetMultipleMethod : SqliteBaseFixture {
			[Test]
			public async Task ReturnsItems() {

				await Db.InsertAsync(new Person { Active = true, FirstName = "a", LastName = "a1", DateCreated = DateTime.UtcNow.AddDays(-10) });
				await Db.InsertAsync(new Person { Active = false, FirstName = "b", LastName = "b1", DateCreated = DateTime.UtcNow.AddDays(-10) });
				await Db.InsertAsync(new Person { Active = true, FirstName = "c", LastName = "c1", DateCreated = DateTime.UtcNow.AddDays(-3) });
				await Db.InsertAsync(new Person { Active = false, FirstName = "d", LastName = "d1", DateCreated = DateTime.UtcNow.AddDays(-1) });

				await Db.InsertAsync(new Animal { Name = "Foo" });
				await Db.InsertAsync(new Animal { Name = "Bar" });
				await Db.InsertAsync(new Animal { Name = "Baz" });

				GetMultiplePredicate predicate = new GetMultiplePredicate();
				predicate.Add<Person>(null);
				predicate.Add<Animal>(Predicates.Field<Animal>(a => a.Name, Operator.Like, "Ba%"));
				predicate.Add<Person>(Predicates.Field<Person>(a => a.LastName, Operator.Eq, "c1"));

				var result = Db.GetMultiple(predicate);
				var people = result.Read<Person>().ToList();
				var animals = result.Read<Animal>().ToList();
				var people2 = result.Read<Person>().ToList();

				Assert.AreEqual(4, people.Count);
				Assert.AreEqual(2, animals.Count);
				Assert.AreEqual(1, people2.Count);

			}
		}
	}
}