using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DapperExtensions;
using DapperExtensions.Mapper;
using System.Reflection;
using Dapper;

namespace DapperExtensions.Mapper
{

	public class AttributeBasedMapper<T> : ClassMapper<T> where T : class
	{

		public AttributeBasedMapper()
		{

			Type type = typeof(T);

			//Table Attribute
			if (type.GetTypeInfo().GetCustomAttributes(true).Any(attr => attr.GetType().Name == typeof(TableAttribute).Name))
			{
				var tn = ((TableAttribute)type.GetTypeInfo().GetCustomAttributes(true).FirstOrDefault(attr => attr.GetType().Name == typeof(TableAttribute).Name)).Name;
				if (tn != "") { TableName = tn; }
				var sn = ((TableAttribute)type.GetTypeInfo().GetCustomAttributes(true).FirstOrDefault(attr => attr.GetType().Name == typeof(TableAttribute).Name)).Schema;
				if (sn != "") { Schema(sn); }
				var selN = ((TableAttribute)type.GetTypeInfo().GetCustomAttributes(true).FirstOrDefault(attr => attr.GetType().Name == typeof(TableAttribute).Name)).NameForSelect;
				if (selN != null && selN != "") { TableNameForSelect = selN; } else
				{
					if (tn != "") TableNameForSelect = tn;
				}
			}

			foreach (var property in type.GetProperties())
			{
				PropertyMap myPM = null;
				//KeyAttribute 
				if (property.GetCustomAttributes(true).Any(attr => attr.GetType().Name == typeof(KeyAttribute).Name))
				{
					var propertyKeyType = ((KeyAttribute)property.GetCustomAttributes(true).FirstOrDefault(attr => attr.GetType().Name == typeof(KeyAttribute).Name)).KeyType;
					myPM = Map(property).Key(propertyKeyType);
				}
				//ColumnAttribute 
				if (property.GetCustomAttributes(true).Any(attr => attr.GetType().Name == typeof(ColumnAttribute).Name))
				{
					var colName = ((ColumnAttribute)property.GetCustomAttributes(true).FirstOrDefault(attr => attr.GetType().Name == typeof(ColumnAttribute).Name)).Name;
					if (colName != null && colName.Trim() != "")
					{
						if (myPM == null) { myPM = Map(property); }
						myPM.Column(colName);
					}
				}
				//ReadOnly attribute (ignore insert/updates)
				if (property.GetCustomAttributes(true).Any(attr => attr.GetType().Name == typeof(ReadOnlyAttribute).Name))
				{
					var readOnly = ((ReadOnlyAttribute)property.GetCustomAttributes(true).FirstOrDefault(attr => attr.GetType().Name == typeof(ReadOnlyAttribute).Name)).IsReadOnly;
					if (readOnly)
					{
						if (myPM == null) { myPM = Map(property); }
						myPM.ReadOnly();
					}
				}

			}

			AutoMap();

		}

		private string GetTableName()
		{
			return "";
		}

	}
}


namespace Dapper
{

	/// <summary>
	/// Optional Table attribute.
	/// You can use the System.ComponentModel.DataAnnotations version in its place to specify the table name of a poco
	/// </summary>
	[AttributeUsage(AttributeTargets.Class)]
	public class TableAttribute : Attribute
	{
		/// <summary>
		/// Optional Table attribute.
		/// </summary>
		/// <param name="tableName"></param>
		/// <param name="tableNameForSelect">If the object uses a query for SELECT, pass the query name here.</param>
		public TableAttribute(string tableName, string tableNameForSelect = "")
		{
			Name = tableName;
			NameForSelect = tableNameForSelect;
		}
		/// <summary>
		/// Name of the table
		/// </summary>
		public string Name { get; private set; }
		/// <summary>
		/// Name of the schema
		/// </summary>
		public string Schema { get; set; }
		/// <summary>
		/// If the table uses a view for SELECT and a table for Create/Update/Delete -- then pass the query name in this parameter.
		/// </summary>
		public string NameForSelect { get; set; }
	}

	/// <summary>
	/// Optional Key attribute.  By default, Key attributes are Identity, but you can specify other types.
	/// </summary>
	[AttributeUsage(AttributeTargets.Property)]
	public class KeyAttribute : Attribute
	{
		public KeyAttribute(KeyType KeyType = KeyType.Identity)
		{
			this.KeyType = KeyType;
		}
		public KeyType KeyType { get; set; }
	}

	/// <summary>
	/// Optional Column attribute.
	/// You can use the System.ComponentModel.DataAnnotations version in its place to specify the table name of a poco
	/// </summary>
	[AttributeUsage(AttributeTargets.Property)]
	public class ColumnAttribute : Attribute
	{
		public ColumnAttribute(string columnName)
		{
			Name = columnName;
		}
		/// Name of the column
		public string Name { get; private set; }
	}

	/// <summary>
	/// Optional Readonly attribute.
	/// You can use the System.ComponentModel version in its place to specify the properties that are editable
	/// </summary>
	[AttributeUsage(AttributeTargets.Property)]
	public class ReadOnlyAttribute : Attribute
	{
		/// <summary>
		/// Optional ReadOnly attribute.
		/// </summary>
		/// <param name="isReadOnly"></param>
		public ReadOnlyAttribute(bool isReadOnly)
		{
			IsReadOnly = isReadOnly;
		}
		/// <summary>
		/// Does this property persist to the database?
		/// </summary>
		public bool IsReadOnly { get; private set; }
	}

}
