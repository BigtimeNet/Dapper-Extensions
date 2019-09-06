using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Numerics;
using System.Reflection;
using DapperExtensions.Mapper;
using DapperExtensions.Test.Helpers;
using NUnit.Framework;

namespace DapperExtensions.Test.Mapper {
	[TestFixture]
	public class AttributeBasedMapperFixture {

        [Test]
        public void Sets_IgnoredPropertyToIgnoredWhenFirstProperty() {
            AttributeBasedMapper<Foo> m = GetMapper<Foo>();
            var map = m.Properties.Single(x => x.Name == "Ignored");
            Assert.AreEqual(true, map.Ignored);
        }

        [Test]
        public void Sets_IgnoredPropertyToIgnoredWhenFoundInClass() {
            AttributeBasedMapper<Bar> m = GetMapper<Bar>();
            var map = m.Properties.Single(x => x.Name == "Ignored");
            Assert.AreEqual(true, map.Ignored);
        }

        [Test]
        public void Sets_NotIgnoredWhenNoIgnoredAttributeFound() {
            AttributeBasedMapper<Baz> m = GetMapper<Baz>();
            Assert.IsTrue(m.Properties.All(x => !x.Ignored));
        }

        private AttributeBasedMapper<T> GetMapper<T>() where T : class {
            return new AttributeBasedMapper<T>();
        }

        private class Foo {
            [Dapper.Ignore]
            public bool Ignored { get; set; }
        }

        private class Bar {
            public string Name { get; set; }
            [Dapper.Ignore]
            public bool Ignored { get; set; }
        }

        private class Baz {
            public bool NotIgnored { get; set; }
        }
    }
}