using System;
using System.Collections.Generic;
using DapperExtensions.Mapper;

namespace DapperExtensions.Test.Data {
	public class Company {
		public int Id { get; set; }
		public string CompanyNm { get; set; }
		public string City { get; set; }
		public string St { get; set; }
		public string Zip { get; set; }
	}

	public class CompanyMapper : ClassMapper<Company> {
		public CompanyMapper() {
			Table("Company");
			AutoMap();
		}
	}
}
