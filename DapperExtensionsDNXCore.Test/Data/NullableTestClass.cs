using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DapperExtensions.Test.Data { 
	public enum ETestEnum {
		First=1, Second=2
	}
    public class NullableTestClass
    {
		public int Id { get; set; }
		public string StringVal { get; set; }
		public string StringVal2 { get; set; }
		public int IntVal { get; set; }
		public int? NullableIntVal { get; set; }
		public DateTime DTVal { get; set; }
		public DateTime? NullableDTVal { get; set; }
		public ETestEnum EnumVal { get; set; }
		public ETestEnum? NullableEnumVal { get; set; }
		public byte[] BlobField { get; set; }
	}
}
