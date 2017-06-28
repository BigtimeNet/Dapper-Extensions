using System.Text;
using System;
using System.Collections.Generic;
using System.Linq;
using DapperExtensions.Test.Data;
using NUnit.Framework;
using System.Reflection;

namespace DapperExtensions.Test.IntegrationTests.SqlServer {
	[TestFixture]
	public class InsertNullableValuesFixture : SqlServerBaseFixture {

		[Test]
		public void InsertSkipsNullableFieldsTest() {

			//StringVal2, NullableDtVal, NullableIntVal and NullableEnumVal are all NULL
			var SampleItem = new Data.NullableTestClass { StringVal = "TestValue", DTVal = DateTime.Now, EnumVal = ETestEnum.First };

			int newId = (int)Db.Insert(SampleItem);
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



		[Test]
		public void InsertVarbinaryData() {

			//StringVal2, NullableDtVal, NullableIntVal and NullableEnumVal are all NULL
			var SampleItem = new Data.NullableTestClass { StringVal="TestValue", DTVal=DateTime.Now, EnumVal= ETestEnum.First};
			var blobData = this.LoadImage();
			Assert.IsNotNull(blobData, "Initial data loaded was null");
			Assert.AreNotEqual(0, blobData.Length);

			SampleItem.BlobField = blobData;
			Assert.AreNotEqual(0, SampleItem.BlobField.Length);

			int newId = (int)Db.Insert(SampleItem);
			Assert.AreNotEqual(0, newId);

			var readSampleItem = Db.Get <NullableTestClass>(newId);
			Assert.IsNotNull(readSampleItem.BlobField, "READ record data was null");
			Assert.AreEqual(blobData.Length, readSampleItem.BlobField.Length);
			Assert.IsTrue(readSampleItem.BlobField.SequenceEqual(blobData));

		}



		private byte[] LoadImage() {

			var assemblyName = new AssemblyName("DapperExtensionsDNXCore.Test");
			var resources = string.Join(Environment.NewLine, Assembly.Load(assemblyName).GetManifestResourceNames());
			Console.WriteLine($"Resource list: {resources}");

			var assembly = Assembly.Load(assemblyName);
			var resourceStream = assembly.GetManifestResourceStream("DapperExtensionsDNXCore.Test.unnamed.png");
			byte[] content = null;
			if(resourceStream==null) { return content; }
			using (var reader = new System.IO.BinaryReader(resourceStream)) {
				content = reader.ReadBytes((int)resourceStream.Length);
			}
			return content;
		}

	}

}