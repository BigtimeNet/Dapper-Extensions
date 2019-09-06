using DapperExtensions.Mapper;
using System;
using System.Collections.Generic;
using System.Text;

namespace DapperExtensions.Test.Data {
    public class City {
        [Dapper.Key(KeyType.Assigned)]
        public string Name { get; set; }
        public string State { get; set; }
        public int Population { get; set; }
        [Dapper.Ignore]
        public string Abbreviation { get; set; }
    }

    public class CityMapper : AttributeBasedMapper<City> {
        public CityMapper() {
           
        }
    }
}
