using System;
using System.Collections.Generic;
using System.Data;
using System.Dynamic;
using System.Linq;
using System.Text;
using Dapper;
using DapperExtensions.Mapper;
using DapperExtensions.Sql;
using System.Data.Common;
using System.Threading.Tasks;
using DapperExtensions.Logging;

namespace DapperExtensions
{
	public interface IDapperImplementor
	{
		ISqlGenerator SqlGenerator { get; }
		ILog Logger { get; set; }
		T Get<T>(IDbConnection connection, dynamic id, DbTransaction transaction, int? commandTimeout) where T : class;
		void Insert<T>(IDbConnection connection, IEnumerable<T> entities, DbTransaction transaction, int? commandTimeout) where T : class;
		dynamic Insert<T>(IDbConnection connection, T entity, DbTransaction transaction, int? commandTimeout) where T : class;
		bool Update<T>(IDbConnection connection, T entity, DbTransaction transaction, int? commandTimeout, bool excludeAssignedKeys = false) where T : class;
		bool Delete<T>(IDbConnection connection, T entity, DbTransaction transaction, int? commandTimeout) where T : class;
		bool Delete<T>(IDbConnection connection, object predicate, DbTransaction transaction, int? commandTimeout) where T : class;
		IEnumerable<T> GetList<T>(IDbConnection connection, object predicate, IList<ISort> sort, DbTransaction transaction, int? commandTimeout, bool buffered) where T : class;
		string GetWhere<T>(IPredicate predicate, Dictionary<string, object> parameters) where T : class;
		IEnumerable<T> GetPage<T>(IDbConnection connection, object predicate, IList<ISort> sort, int page, int resultsPerPage, DbTransaction transaction, int? commandTimeout, bool buffered) where T : class;
		IEnumerable<T> GetSet<T>(IDbConnection connection, object predicate, IList<ISort> sort, int firstResult, int maxResults, DbTransaction transaction, int? commandTimeout, bool buffered) where T : class;
		int Count<T>(IDbConnection connection, object predicate, DbTransaction transaction, int? commandTimeout) where T : class;
		IMultipleResultReader GetMultiple(IDbConnection connection, GetMultiplePredicate predicate, DbTransaction transaction, int? commandTimeout);

		Task<T> GetAsync<T>(IDbConnection connection, dynamic id, IDbTransaction transaction = null, int? commandTimeout = null) where T : class;
		Task<IEnumerable<T>> GetListAsync<T>(IDbConnection connection, object predicate = null, IList<ISort> sort = null, IDbTransaction transaction = null, int? commandTimeout = null) where T : class;
		Task<IEnumerable<T>> GetPageAsync<T>(IDbConnection connection, object predicate = null, IList<ISort> sort = null, int page = 1, int resultsPerPage = 10, IDbTransaction transaction = null, int? commandTimeout = null) where T : class;
		Task<IEnumerable<T>> GetSetAsync<T>(IDbConnection connection, object predicate = null, IList<ISort> sort = null, int firstResult = 1, int maxResults = 10, IDbTransaction transaction = null, int? commandTimeout = null) where T : class;
		Task<int> CountAsync<T>(IDbConnection connection, object predicate = null, IDbTransaction transaction = null, int? commandTimeout = null) where T : class;
		Task InsertAsync<T>(IDbConnection connection, IEnumerable<T> entities, DbTransaction transaction, int? commandTimeout) where T : class;
		Task<dynamic> InsertAsync<T>(IDbConnection connection, T entity, DbTransaction transaction, int? commandTimeout) where T : class;
		Task<bool> UpdateAsync<T>(IDbConnection connection, T entity, DbTransaction transaction, int? commandTimeout, bool excludeAssignedKeys = false) where T : class;
		Task<bool> DeleteAsync<T>(IDbConnection connection, T entity, DbTransaction transaction, int? commandTimeout) where T : class;
		Task<bool> DeleteAsync<T>(IDbConnection connection, object predicate, DbTransaction transaction, int? commandTimeout) where T : class;

	}

	public class DapperImplementor : IDapperImplementor
	{
		public DapperImplementor(ISqlGenerator sqlGenerator)
		{
			SqlGenerator = sqlGenerator;
		}

		public ISqlGenerator SqlGenerator { get; private set; }
		public ILog Logger { get; set; }

		public T Get<T>(IDbConnection connection, dynamic id, DbTransaction transaction, int? commandTimeout) where T : class
		{
			IClassMapper classMap = SqlGenerator.Configuration.GetMap<T>();
			IPredicate predicate = GetIdPredicate(classMap, id);
			T result = GetList<T>(connection, classMap, predicate, null, transaction, commandTimeout, true).SingleOrDefault();
			return result;
		}

		public void Insert<T>(IDbConnection connection, IEnumerable<T> entities, DbTransaction transaction, int? commandTimeout) where T : class
		{
			IClassMapper classMap = SqlGenerator.Configuration.GetMap<T>();
			var properties = classMap.Properties.Where(p => p.KeyType != KeyType.NotAKey);
			foreach (var e in entities)
			{
				foreach (var column in properties)
				{
					if (column.KeyType == KeyType.Guid && (Guid)column.PropertyInfo.GetValue(e, null) == Guid.Empty)
					{
						Guid comb = SqlGenerator.Configuration.GetNextGuid();
						column.PropertyInfo.SetValue(e, comb, null);
					}
				}
			}
			string sql = SqlGenerator.Insert(classMap);
			if (this.Logger != null) { this.Logger.Log(sql); }
			connection.Execute(sql, entities, transaction, commandTimeout, CommandType.Text);
		}

		public dynamic Insert<T>(IDbConnection connection, T entity, DbTransaction transaction, int? commandTimeout) where T : class
		{
			IClassMapper classMap = SqlGenerator.Configuration.GetMap<T>();
			List<IPropertyMap> nonIdentityKeyProperties = classMap.Properties.Where(p => p.KeyType == KeyType.Guid || p.KeyType == KeyType.Assigned).ToList();
			var identityColumn = classMap.Properties.SingleOrDefault(p => p.KeyType == KeyType.Identity);
			foreach (var column in nonIdentityKeyProperties)
			{
				if (column.KeyType == KeyType.Guid && (Guid)column.PropertyInfo.GetValue(entity, null) == Guid.Empty)
				{
					Guid comb = SqlGenerator.Configuration.GetNextGuid();
					column.PropertyInfo.SetValue(entity, comb, null);
				}
			}

			IDictionary<string, object> keyValues = new ExpandoObject();
			string sql = SqlGenerator.Insert(classMap, entity);
			if (identityColumn != null)
			{
				IEnumerable<long> result;
				if (SqlGenerator.SupportsMultipleStatements())
				{
					sql += SqlGenerator.Configuration.Dialect.BatchSeperator + SqlGenerator.IdentitySql(classMap);
					if (this.Logger != null) { this.Logger.Log(sql); }
					result = connection.Query<long>(sql, entity, transaction, false, commandTimeout, CommandType.Text);
				} else
				{
					if (this.Logger != null) { this.Logger.Log(sql); }
					connection.Execute(sql, entity, transaction, commandTimeout, CommandType.Text);
					sql = SqlGenerator.IdentitySql(classMap);
					if (this.Logger != null) { this.Logger.Log(sql); }
					result = connection.Query<long>(sql, entity, transaction, false, commandTimeout, CommandType.Text);
				}
				long identityValue = result.First();
				keyValues.Add(identityColumn.Name, identityValue);
				try
				{
					identityColumn.PropertyInfo.SetValue(entity,
						Convert.ChangeType(identityValue, identityColumn.PropertyInfo.PropertyType), null);
				} catch (Exception)
				{
					//Ignore error if we can't auto-set property from Id returned
				}
			} else
			{
				if (this.Logger != null) { this.Logger.Log(sql); }
				connection.Execute(sql, entity, transaction, commandTimeout, CommandType.Text);
			}

			foreach (var column in nonIdentityKeyProperties)
			{
				keyValues.Add(column.Name, column.PropertyInfo.GetValue(entity, null));
			}

			if (keyValues.Count == 1)
			{
				return keyValues.First().Value;
			}

			return keyValues;

		}

		public bool Update<T>(IDbConnection connection, T entity, DbTransaction transaction, int? commandTimeout, bool excludeAssignedKeys = false) where T : class
		{
			IClassMapper classMap = SqlGenerator.Configuration.GetMap<T>();
			IPredicate predicate = GetKeyPredicate<T>(classMap, entity);
			Dictionary<string, object> parameters = new Dictionary<string, object>();
			string sql = SqlGenerator.Update(classMap, predicate, parameters, excludeAssignedKeys);
			DynamicParameters dynamicParameters = new DynamicParameters();

			//Columns to update are the columns that are NOT ignored, readOnly or identity
			//AND, if ExcludeAssignedKeys = true, then also columns that are KeyType assigned.
			var columns = classMap.Properties.Where(p => !(
				p.Ignored ||
				p.IsReadOnly ||
				p.KeyType == KeyType.Identity ||
				(excludeAssignedKeys && p.KeyType == KeyType.Assigned)
			));
			foreach (var property in ReflectionHelper.GetObjectValues(entity).Where(property => columns.Any(c => c.Name == property.Key)))
			{
				dynamicParameters.Add(property.Key, property.Value);
			}

			foreach (var parameter in parameters)
			{
				dynamicParameters.Add(parameter.Key, parameter.Value);
			}

			if (this.Logger != null) { this.Logger.Log(sql); }
			return connection.Execute(sql, dynamicParameters, transaction, commandTimeout, CommandType.Text) > 0;
		}

		public bool Delete<T>(IDbConnection connection, T entity, DbTransaction transaction, int? commandTimeout) where T : class
		{
			IClassMapper classMap = SqlGenerator.Configuration.GetMap<T>();
			IPredicate predicate = GetKeyPredicate<T>(classMap, entity);
			return Delete<T>(connection, classMap, predicate, transaction, commandTimeout);
		}

		public bool Delete<T>(IDbConnection connection, object predicate, DbTransaction transaction, int? commandTimeout) where T : class
		{
			IClassMapper classMap = SqlGenerator.Configuration.GetMap<T>();
			IPredicate wherePredicate = GetPredicate(classMap, predicate);
			return Delete<T>(connection, classMap, wherePredicate, transaction, commandTimeout);
		}

		public IEnumerable<T> GetList<T>(IDbConnection connection, object predicate, IList<ISort> sort, DbTransaction transaction, int? commandTimeout, bool buffered) where T : class
		{
			IClassMapper classMap = SqlGenerator.Configuration.GetMap<T>();
			IPredicate wherePredicate = GetPredicate(classMap, predicate);
			return GetList<T>(connection, classMap, wherePredicate, sort, transaction, commandTimeout, true);
		}

		public string GetWhere<T>(IPredicate predicate, Dictionary<string,object> parameters) where T : class
		{
			IClassMapper classMap = SqlGenerator.Configuration.GetMap<T>();
			IPredicate wherePredicate = GetPredicate(classMap, predicate);
			return wherePredicate.GetSql(SqlGenerator, parameters);
		}

		public IEnumerable<T> GetPage<T>(IDbConnection connection, object predicate, IList<ISort> sort, int page, int resultsPerPage, DbTransaction transaction, int? commandTimeout, bool buffered) where T : class
		{
			IClassMapper classMap = SqlGenerator.Configuration.GetMap<T>();
			IPredicate wherePredicate = GetPredicate(classMap, predicate);
			return GetPage<T>(connection, classMap, wherePredicate, sort, page, resultsPerPage, transaction, commandTimeout, buffered);
		}

		public IEnumerable<T> GetSet<T>(IDbConnection connection, object predicate, IList<ISort> sort, int firstResult, int maxResults, DbTransaction transaction, int? commandTimeout, bool buffered) where T : class
		{
			IClassMapper classMap = SqlGenerator.Configuration.GetMap<T>();
			IPredicate wherePredicate = GetPredicate(classMap, predicate);
			return GetSet<T>(connection, classMap, wherePredicate, sort, firstResult, maxResults, transaction, commandTimeout, buffered);
		}

		public int Count<T>(IDbConnection connection, object predicate, DbTransaction transaction, int? commandTimeout) where T : class
		{
			IClassMapper classMap = SqlGenerator.Configuration.GetMap<T>();
			IPredicate wherePredicate = GetPredicate(classMap, predicate);
			Dictionary<string, object> parameters = new Dictionary<string, object>();
			string sql = SqlGenerator.Count(classMap, wherePredicate, parameters);
			if (this.Logger != null) { this.Logger.Log(sql); }
			DynamicParameters dynamicParameters = new DynamicParameters();
			foreach (var parameter in parameters)
			{
				dynamicParameters.Add(parameter.Key, parameter.Value);
			}

			if (this.Logger != null) { this.Logger.Log(sql); }
			return (int)connection.Query(sql, dynamicParameters, transaction, false, commandTimeout, CommandType.Text).Single().Total;
		}

		public IMultipleResultReader GetMultiple(IDbConnection connection, GetMultiplePredicate predicate, DbTransaction transaction, int? commandTimeout)
		{
			if (SqlGenerator.SupportsMultipleStatements())
			{
				return GetMultipleByBatch(connection, predicate, transaction, commandTimeout);
			}

			return GetMultipleBySequence(connection, predicate, transaction, commandTimeout);
		}

		/// <summary>
		/// The asynchronous counterpart to <see cref="IDapperImplementor.Get{T}"/>.
		/// </summary>
		public Task<T> GetAsync<T>(IDbConnection connection, dynamic id, IDbTransaction transaction = null,
			 int? commandTimeout = null) where T : class
		{
			IClassMapper classMap = SqlGenerator.Configuration.GetMap<T>();
			IPredicate predicate = GetIdPredicate(classMap, id);
			return GetListAsync<T>(connection, classMap, predicate, null, transaction, commandTimeout).ContinueWith(t => t.Result.SingleOrDefault());
			//return getList.SingleOrDefault();
		}

		/// <summary>
		/// The asynchronous counterpart to <see cref="IDapperImplementor.GetList{T}"/>.
		/// </summary>
		public async Task<IEnumerable<T>> GetListAsync<T>(IDbConnection connection, object predicate = null, IList<ISort> sort = null,
			 IDbTransaction transaction = null, int? commandTimeout = null) where T : class
		{
			IClassMapper classMap = SqlGenerator.Configuration.GetMap<T>();
			IPredicate wherePredicate = GetPredicate(classMap, predicate);
			return await GetListAsync<T>(connection, classMap, wherePredicate, sort, transaction, commandTimeout);
		}

		/// <summary>
		/// The asynchronous counterpart to <see cref="IDapperImplementor.GetPage{T}"/>.
		/// </summary>
		public async Task<IEnumerable<T>> GetPageAsync<T>(IDbConnection connection, object predicate = null, IList<ISort> sort = null, int page = 1,
			 int resultsPerPage = 10, IDbTransaction transaction = null, int? commandTimeout = null) where T : class
		{
			IClassMapper classMap = SqlGenerator.Configuration.GetMap<T>();
			IPredicate wherePredicate = GetPredicate(classMap, predicate);
			return await GetPageAsync<T>(connection, classMap, wherePredicate, sort, page, resultsPerPage, transaction, commandTimeout);
		}

		/// <summary>
		/// The asynchronous counterpart to <see cref="IDapperImplementor.GetSet{T}"/>.
		/// </summary>
		public async Task<IEnumerable<T>> GetSetAsync<T>(IDbConnection connection, object predicate = null, IList<ISort> sort = null, int firstResult = 1,
			 int maxResults = 10, IDbTransaction transaction = null, int? commandTimeout = null) where T : class
		{
			IClassMapper classMap = SqlGenerator.Configuration.GetMap<T>();
			IPredicate wherePredicate = GetPredicate(classMap, predicate);
			return await GetSetAsync<T>(connection, classMap, wherePredicate, sort, firstResult, maxResults, transaction, commandTimeout);
		}

		/// <summary>
		/// The asynchronous counterpart to <see cref="IDapperImplementor.Count{T}"/>.
		/// </summary>
		public async Task<int> CountAsync<T>(IDbConnection connection, object predicate = null, IDbTransaction transaction = null,
			 int? commandTimeout = null) where T : class
		{
			IClassMapper classMap = SqlGenerator.Configuration.GetMap<T>();
			IPredicate wherePredicate = GetPredicate(classMap, predicate);
			Dictionary<string, object> parameters = new Dictionary<string, object>();
			string sql = SqlGenerator.Count(classMap, wherePredicate, parameters);
			DynamicParameters dynamicParameters = new DynamicParameters();
			foreach (var parameter in parameters)
			{
				dynamicParameters.Add(parameter.Key, parameter.Value);
			}

			if (this.Logger != null) { this.Logger.Log(sql); }
			return (int)(await connection.QueryAsync(sql, dynamicParameters, transaction, commandTimeout, CommandType.Text)).Single().Total;
		}

		public async Task InsertAsync<T>(IDbConnection connection, IEnumerable<T> entities, DbTransaction transaction, int? commandTimeout) where T : class
		{

			IClassMapper classMap = SqlGenerator.Configuration.GetMap<T>();
			var properties = classMap.Properties.Where(p => p.KeyType != KeyType.NotAKey);

			foreach (var e in entities)
			{
				foreach (var column in properties)
				{
					if (column.KeyType == KeyType.Guid && (Guid)column.PropertyInfo.GetValue(e, null) == Guid.Empty)
					{
						Guid comb = SqlGenerator.Configuration.GetNextGuid();
						column.PropertyInfo.SetValue(e, comb, null);
					}
				}
			}

			string sql = SqlGenerator.Insert(classMap);

			if (this.Logger != null) { this.Logger.Log(sql); }
			await connection.ExecuteAsync(sql, entities, transaction, commandTimeout, CommandType.Text);

		}

		public async Task<dynamic> InsertAsync<T>(IDbConnection connection, T entity, DbTransaction transaction, int? commandTimeout) where T : class
		{

			IClassMapper classMap = SqlGenerator.Configuration.GetMap<T>();
			List<IPropertyMap> nonIdentityKeyProperties = classMap.Properties.Where(p => p.KeyType == KeyType.Guid || p.KeyType == KeyType.Assigned).ToList();
			var identityColumn = classMap.Properties.SingleOrDefault(p => p.KeyType == KeyType.Identity);
			foreach (var column in nonIdentityKeyProperties)
			{
				if (column.KeyType == KeyType.Guid && (Guid)column.PropertyInfo.GetValue(entity, null) == Guid.Empty)
				{
					Guid comb = SqlGenerator.Configuration.GetNextGuid();
					column.PropertyInfo.SetValue(entity, comb, null);
				}
			}

			IDictionary<string, object> keyValues = new ExpandoObject();
			string sql = SqlGenerator.Insert(classMap, entity);
			if (this.Logger != null) { this.Logger.Log(sql); }
			if (identityColumn != null)
			{
				IEnumerable<long> result;
				if (SqlGenerator.SupportsMultipleStatements())
				{
					sql += SqlGenerator.Configuration.Dialect.BatchSeperator + SqlGenerator.IdentitySql(classMap);
					result = await connection.QueryAsync<long>(sql, entity, transaction, commandTimeout, CommandType.Text);
				} else
				{
					connection.Execute(sql, entity, transaction, commandTimeout, CommandType.Text);
					sql = SqlGenerator.IdentitySql(classMap);
					result = await connection.QueryAsync<long>(sql, entity, transaction, commandTimeout, CommandType.Text);
				}

				long identityValue = result.First();
				keyValues.Add(identityColumn.Name, identityValue);
				try
				{
					identityColumn.PropertyInfo.SetValue(entity, Convert.ChangeType(identityValue, identityColumn.PropertyInfo.PropertyType), null);
				} catch (Exception)
				{
					//Ignore exception if user can't set property value
				}

			} else
			{
				connection.Execute(sql, entity, transaction, commandTimeout, CommandType.Text);
			}

			foreach (var column in nonIdentityKeyProperties)
			{
				keyValues.Add(column.Name, column.PropertyInfo.GetValue(entity, null));
			}

			if (keyValues.Count == 1)
			{
				return keyValues.First().Value;
			}

			return keyValues;

		}

		public async Task<bool> UpdateAsync<T>(IDbConnection connection, T entity, DbTransaction transaction, int? commandTimeout, bool excludeAssignedKeys = false) where T : class
		{
			IClassMapper classMap = SqlGenerator.Configuration.GetMap<T>();
			IPredicate predicate = GetKeyPredicate<T>(classMap, entity);
			Dictionary<string, object> parameters = new Dictionary<string, object>();
			string sql = SqlGenerator.Update(classMap, predicate, parameters, excludeAssignedKeys);
			DynamicParameters dynamicParameters = new DynamicParameters();

			var columns = classMap.Properties.Where(p => !(p.Ignored || p.IsReadOnly || p.KeyType == KeyType.Identity || (excludeAssignedKeys && p.KeyType == KeyType.Assigned)));
			foreach (var property in ReflectionHelper.GetObjectValues(entity).Where(property => columns.Any(c => c.Name == property.Key)))
			{
				dynamicParameters.Add(property.Key, property.Value);
			}

			foreach (var parameter in parameters)
			{
				dynamicParameters.Add(parameter.Key, parameter.Value);
			}

			if (this.Logger != null) { this.Logger.Log(sql); }
			return (await connection.ExecuteAsync(sql, dynamicParameters, transaction, commandTimeout, CommandType.Text)) > 0;

		}

		public Task<bool> DeleteAsync<T>(IDbConnection connection, T entity, DbTransaction transaction, int? commandTimeout) where T : class
		{
			IClassMapper classMap = SqlGenerator.Configuration.GetMap<T>();
			IPredicate predicate = GetKeyPredicate<T>(classMap, entity);
			return DeleteAsync<T>(connection, classMap, predicate, transaction, commandTimeout);
		}

		public Task<bool> DeleteAsync<T>(IDbConnection connection, object predicate, DbTransaction transaction, int? commandTimeout) where T : class
		{
			IClassMapper classMap = SqlGenerator.Configuration.GetMap<T>();
			IPredicate wherePredicate = GetPredicate(classMap, predicate);
			return DeleteAsync<T>(connection, classMap, wherePredicate, transaction, commandTimeout);
		}

		protected IEnumerable<T> GetList<T>(IDbConnection connection, IClassMapper classMap, IPredicate predicate, IList<ISort> sort, DbTransaction transaction, int? commandTimeout, bool buffered) where T : class
		{
			Dictionary<string, object> parameters = new Dictionary<string, object>();
			string sql = SqlGenerator.Select(classMap, predicate, sort, parameters);
			DynamicParameters dynamicParameters = new DynamicParameters();
			foreach (var parameter in parameters)
			{
				dynamicParameters.Add(parameter.Key, parameter.Value);
			}

			if (this.Logger != null) { this.Logger.Log(sql); }
			return connection.Query<T>(sql, dynamicParameters, transaction, buffered, commandTimeout, CommandType.Text);
		}

		protected IEnumerable<T> GetPage<T>(IDbConnection connection, IClassMapper classMap, IPredicate predicate, IList<ISort> sort, int page, int resultsPerPage, DbTransaction transaction, int? commandTimeout, bool buffered) where T : class
		{
			Dictionary<string, object> parameters = new Dictionary<string, object>();
			string sql = SqlGenerator.SelectPaged(classMap, predicate, sort, page, resultsPerPage, parameters);
			DynamicParameters dynamicParameters = new DynamicParameters();
			foreach (var parameter in parameters)
			{
				dynamicParameters.Add(parameter.Key, parameter.Value);
			}

			if (this.Logger != null) { this.Logger.Log(sql); }
			return connection.Query<T>(sql, dynamicParameters, transaction, buffered, commandTimeout, CommandType.Text);
		}

		protected IEnumerable<T> GetSet<T>(IDbConnection connection, IClassMapper classMap, IPredicate predicate, IList<ISort> sort, int firstResult, int maxResults, DbTransaction transaction, int? commandTimeout, bool buffered) where T : class
		{
			Dictionary<string, object> parameters = new Dictionary<string, object>();
			string sql = SqlGenerator.SelectSet(classMap, predicate, sort, firstResult, maxResults, parameters);
			DynamicParameters dynamicParameters = new DynamicParameters();
			foreach (var parameter in parameters)
			{
				dynamicParameters.Add(parameter.Key, parameter.Value);
			}

			if (this.Logger != null) { this.Logger.Log(sql); }
			return connection.Query<T>(sql, dynamicParameters, transaction, buffered, commandTimeout, CommandType.Text);
		}

		protected bool Delete<T>(IDbConnection connection, IClassMapper classMap, IPredicate predicate, DbTransaction transaction, int? commandTimeout) where T : class
		{
			Dictionary<string, object> parameters = new Dictionary<string, object>();
			string sql = SqlGenerator.Delete(classMap, predicate, parameters);
			DynamicParameters dynamicParameters = new DynamicParameters();
			foreach (var parameter in parameters)
			{
				dynamicParameters.Add(parameter.Key, parameter.Value);
			}

			if (this.Logger != null) { this.Logger.Log(sql); }
			connection.Execute(sql, dynamicParameters, transaction, commandTimeout, CommandType.Text);
			return true;
		}

		protected IPredicate GetPredicate(IClassMapper classMap, object predicate)
		{
			IPredicate wherePredicate = predicate as IPredicate;
			if (wherePredicate == null && predicate != null)
			{
				wherePredicate = GetEntityPredicate(classMap, predicate);
			}

			return wherePredicate;
		}

		protected IPredicate GetIdPredicate(IClassMapper classMap, object id)
		{
			bool isSimpleType = ReflectionHelper.IsSimpleType(id.GetType());
			var keys = classMap.Properties.Where(p => p.KeyType != KeyType.NotAKey);
			IDictionary<string, object> paramValues = null;
			IList<IPredicate> predicates = new List<IPredicate>();
			if (!isSimpleType)
			{
				paramValues = ReflectionHelper.GetObjectValues(id);
			}

			foreach (var key in keys)
			{
				object value = id;
				if (!isSimpleType)
				{
					value = paramValues[key.Name];
				}

				Type predicateType = typeof(FieldPredicate<>).MakeGenericType(classMap.EntityType);

				IFieldPredicate fieldPredicate = Activator.CreateInstance(predicateType) as IFieldPredicate;
				fieldPredicate.Not = false;
				fieldPredicate.Operator = Operator.Eq;
				fieldPredicate.PropertyName = key.Name;
				fieldPredicate.Value = value;
				predicates.Add(fieldPredicate);
			}

			return predicates.Count == 1
						  ? predicates[0]
						  : new PredicateGroup
						  {
							  Operator = GroupOperator.And,
							  Predicates = predicates
						  };
		}

		protected IPredicate GetKeyPredicate<T>(IClassMapper classMap, T entity) where T : class
		{
			var whereFields = classMap.Properties.Where(p => p.KeyType != KeyType.NotAKey);
			if (!whereFields.Any())
			{
				throw new ArgumentException("At least one Key column must be defined.");
			}

			IList<IPredicate> predicates = (from field in whereFields
													  select new FieldPredicate<T>
													  {
														  Not = false,
														  Operator = Operator.Eq,
														  PropertyName = field.Name,
														  Value = field.PropertyInfo.GetValue(entity, null)
													  }).Cast<IPredicate>().ToList();

			return predicates.Count == 1
						  ? predicates[0]
						  : new PredicateGroup
						  {
							  Operator = GroupOperator.And,
							  Predicates = predicates
						  };
		}

		protected IPredicate GetEntityPredicate(IClassMapper classMap, object entity)
		{
			Type predicateType = typeof(FieldPredicate<>).MakeGenericType(classMap.EntityType);
			IList<IPredicate> predicates = new List<IPredicate>();
			foreach (var kvp in ReflectionHelper.GetObjectValues(entity))
			{
				IFieldPredicate fieldPredicate = Activator.CreateInstance(predicateType) as IFieldPredicate;
				fieldPredicate.Not = false;
				fieldPredicate.Operator = Operator.Eq;
				fieldPredicate.PropertyName = kvp.Key;
				fieldPredicate.Value = kvp.Value;
				predicates.Add(fieldPredicate);
			}

			return predicates.Count == 1
						  ? predicates[0]
						  : new PredicateGroup
						  {
							  Operator = GroupOperator.And,
							  Predicates = predicates
						  };
		}

		protected GridReaderResultReader GetMultipleByBatch(IDbConnection connection, GetMultiplePredicate predicate, DbTransaction transaction, int? commandTimeout)
		{
			Dictionary<string, object> parameters = new Dictionary<string, object>();
			StringBuilder sql = new StringBuilder();
			foreach (var item in predicate.Items)
			{
				IClassMapper classMap = SqlGenerator.Configuration.GetMap(item.Type);
				IPredicate itemPredicate = item.Value as IPredicate;
				if (itemPredicate == null && item.Value != null)
				{
					itemPredicate = GetPredicate(classMap, item.Value);
				}

				sql.AppendLine(SqlGenerator.Select(classMap, itemPredicate, item.Sort, parameters) + SqlGenerator.Configuration.Dialect.BatchSeperator);
			}

			DynamicParameters dynamicParameters = new DynamicParameters();
			foreach (var parameter in parameters)
			{
				dynamicParameters.Add(parameter.Key, parameter.Value);
			}

			SqlMapper.GridReader grid = connection.QueryMultiple(sql.ToString(), dynamicParameters, transaction, commandTimeout, CommandType.Text);
			return new GridReaderResultReader(grid);
		}

		protected SequenceReaderResultReader GetMultipleBySequence(IDbConnection connection, GetMultiplePredicate predicate, DbTransaction transaction, int? commandTimeout)
		{
			IList<SqlMapper.GridReader> items = new List<SqlMapper.GridReader>();
			foreach (var item in predicate.Items)
			{
				Dictionary<string, object> parameters = new Dictionary<string, object>();
				IClassMapper classMap = SqlGenerator.Configuration.GetMap(item.Type);
				IPredicate itemPredicate = item.Value as IPredicate;
				if (itemPredicate == null && item.Value != null)
				{
					itemPredicate = GetPredicate(classMap, item.Value);
				}

				string sql = SqlGenerator.Select(classMap, itemPredicate, item.Sort, parameters);
				DynamicParameters dynamicParameters = new DynamicParameters();
				foreach (var parameter in parameters)
				{
					dynamicParameters.Add(parameter.Key, parameter.Value);
				}

				SqlMapper.GridReader queryResult = connection.QueryMultiple(sql, dynamicParameters, transaction, commandTimeout, CommandType.Text);
				items.Add(queryResult);
			}

			return new SequenceReaderResultReader(items);
		}

		protected async Task<IEnumerable<T>> GetListAsync<T>(IDbConnection connection, IClassMapper classMap, IPredicate predicate, IList<ISort> sort, IDbTransaction transaction, int? commandTimeout) where T : class
		{
			Dictionary<string, object> parameters = new Dictionary<string, object>();
			string sql = SqlGenerator.Select(classMap, predicate, sort, parameters);
			DynamicParameters dynamicParameters = new DynamicParameters();
			foreach (var parameter in parameters)
			{
				dynamicParameters.Add(parameter.Key, parameter.Value);
			}
			var myList = await connection.QueryAsync<T>(sql, dynamicParameters, transaction, commandTimeout, CommandType.Text);
			return myList;
		}

		/// <summary>
		/// The asynchronous counterpart to <see cref="IDapperImplementor.GetPage{T}"/>.
		/// </summary>
		protected async Task<IEnumerable<T>> GetPageAsync<T>(IDbConnection connection, IClassMapper classMap, IPredicate predicate, IList<ISort> sort, int page, int resultsPerPage, IDbTransaction transaction, int? commandTimeout) where T : class
		{
			Dictionary<string, object> parameters = new Dictionary<string, object>();
			string sql = SqlGenerator.SelectPaged(classMap, predicate, sort, page, resultsPerPage, parameters);
			DynamicParameters dynamicParameters = new DynamicParameters();
			foreach (var parameter in parameters)
			{
				dynamicParameters.Add(parameter.Key, parameter.Value);
			}

			return await connection.QueryAsync<T>(sql, dynamicParameters, transaction, commandTimeout, CommandType.Text);
		}

		/// <summary>
		/// The asynchronous counterpart to <see cref="IDapperImplementor.GetSet{T}"/>.
		/// </summary>
		protected async Task<IEnumerable<T>> GetSetAsync<T>(IDbConnection connection, IClassMapper classMap, IPredicate predicate, IList<ISort> sort, int firstResult, int maxResults, IDbTransaction transaction, int? commandTimeout) where T : class
		{
			Dictionary<string, object> parameters = new Dictionary<string, object>();
			string sql = SqlGenerator.SelectSet(classMap, predicate, sort, firstResult, maxResults, parameters);
			DynamicParameters dynamicParameters = new DynamicParameters();
			foreach (var parameter in parameters)
			{
				dynamicParameters.Add(parameter.Key, parameter.Value);
			}

			return await connection.QueryAsync<T>(sql, dynamicParameters, transaction, commandTimeout, CommandType.Text);
		}

		async protected Task<bool> DeleteAsync<T>(IDbConnection connection, IClassMapper classMap, IPredicate predicate, DbTransaction transaction, int? commandTimeout) where T : class
		{
			Dictionary<string, object> parameters = new Dictionary<string, object>();
			string sql = SqlGenerator.Delete(classMap, predicate, parameters);
			DynamicParameters dynamicParameters = new DynamicParameters();
			foreach (var parameter in parameters)
			{
				dynamicParameters.Add(parameter.Key, parameter.Value);
			}

			await connection.ExecuteAsync(sql, dynamicParameters, transaction, commandTimeout, CommandType.Text);
			return true;
		}

	}
}
