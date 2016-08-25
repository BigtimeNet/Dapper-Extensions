using System.Text;
using System;
using System.Collections.Generic;
using System.Linq;
using DapperExtensions.Test.Data;
using NUnit.Framework;

namespace DapperExtensions.Test.IntegrationTests.SqlServer {
	[TestFixture]
	public class InsertNullableValuesFixture : SqlServerBaseFixture {

		[Test]
		public void InsertSkipsNullableFieldsTest() {

			//StringVal2, NullableDtVal, NullableIntVal and NullableEnumVal are all NULL
			var SampleItem = new Data.NullableTestClass { StringVal = "TestValue", DTVal = DateTime.Now, EnumVal = ETestEnum.First };

			int newId = Db.Insert(SampleItem);
			Assert.AreNotEqual(0, newId);

			var readSampleItem = Db.Get<NullableTestClass>(newId);
			Assert.AreEqual(SampleItem.StringVal, readSampleItem.StringVal);
			Assert.IsNull(readSampleItem.StringVal2);
			Assert.AreEqual(SampleItem.DTVal.ToString("yyyyMMdd"), readSampleItem.DTVal.ToString("yyyyMMdd"));
			//Note that NullableDtVal has a DEFAULT value -- we want to make sure that default is filled in
			Assert.IsNotNull(readSampleItem.NullableDTVal);
			Assert.AreEqual(SampleItem.IntVal, readSampleItem.IntVal);
			//Note that NullableIntVal has a DEFAULT value -- we want to make sure that default is filled in
			Assert.IsNotNull(readSampleItem.NullableIntVal);
			Assert.AreEqual(SampleItem.EnumVal, readSampleItem.EnumVal);
			Assert.IsNull(readSampleItem.NullableEnumVal);

		}

	}


}