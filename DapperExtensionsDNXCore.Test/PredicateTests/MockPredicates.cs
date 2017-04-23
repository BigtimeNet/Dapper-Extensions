using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DapperExtensions.Sql;
using DapperExtensions;

namespace DapperExtensionsDNXCore.Test.PredicateTests
{
	public class MockPredicate : DapperExtensions.BasePredicate
	{
		public string SQL { get; set; }

		public override string GetSql(ISqlGenerator sqlGenerator, IDictionary<string, object> parameters)
		{
			return SQL;
		}

		public string GetColumnNameTester(Type entityType, ISqlGenerator sqlGenerator, string propertyName)
		{
			return base.GetColumnName(entityType, sqlGenerator, propertyName);
		}
	}

	internal class MockPredicateCompare:ComparePredicate {
		public string SQL { get; set; }

		public override string GetSql(ISqlGenerator sqlGenerator, IDictionary<string, object> parameters)
		{
			return SQL;
		}

	}

}