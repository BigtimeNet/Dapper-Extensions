using DapperExtensions.Mapper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace DapperExtensionsDNXCore.Test.PredicateTests
{
	public class MockClassMapper<T> : IClassMapper
	{
		public Type EntityType
		{
			get
			{
				return typeof(T);
			}
		}

		IList<IPropertyMap> _pList;
		public IList<IPropertyMap> Properties
		{
			get
			{
				if(_pList==null)
				{
					_pList = new List<IPropertyMap>();
					foreach (var p in this.EntityType.GetProperties())
					{
						_pList.Add(new PropertyMap(p));
					}
				}
				return _pList;
			}
		}

		public string SchemaName
		{
			get
			{
				return "";
			}
		}

		public string TableName
		{
			get
			{
				return "Table";
			}
		}

		public string TableNameForSelect
		{
			get
			{
				return "SelectTable";
			}
		}
	}
}
