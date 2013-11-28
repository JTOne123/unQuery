﻿using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Dynamic;
using System.Reflection;
using unQuery.SqlTypes;

namespace unQuery
{
	/*
	 * Access methods
	 *  - Get multiple result sets with multiple rows
	 *  
	 * Query methods
	 *	- Stored procedure
	 *	- Text
	 *	
	 * Misc
	 *  - Way to override standard CLR type handlers
	 *  - VisibleFieldCount vs FieldCount
	 */

	public abstract class unQuery
	{
		protected abstract string ConnectionString { get; }

		/// <summary>
		/// Executes the batch and returns all rows from the single result set.
		/// </summary>
		/// <param name="sql">The SQL statement to execute.</param>
		public IList<dynamic> GetRows(string sql)
		{
			return GetRows(sql, null);
		}

		/// <summary>
		/// Executes the batch and returns all rows from the single result set.
		/// </summary>
		/// <param name="sql">The SQL statement to execute.</param>
		/// <param name="parameters">Anonymous object providing parameters for the query.</param>
		public IList<dynamic> GetRows(string sql, dynamic parameters)
		{
			var result = new List<dynamic>();

			using (var conn = getConnection())
			using (var cmd = new SqlCommand(sql, conn))
			{
				if (parameters != null)
					AddParametersToCommand(cmd, parameters);

				var reader = cmd.ExecuteReader(CommandBehavior.SingleResult);

				while (reader.Read())
					result.Add(MapReaderRowToObject(reader));
			}

			return result;
		}

		/// <summary>
		/// Executes the batch and returns a single row of data. If more than one row is is returned from the database,
		/// all but the first will be discarded.
		/// </summary>
		/// <param name="sql">The SQL statement to execute.</param>
		public dynamic GetRow(string sql)
		{
			return GetRow(sql, null);
		}

		/// <summary>
		/// Executes the batch and returns a single row of data. If more than one row is is returned from the database,
		/// all but the first will be discarded.
		/// </summary>
		/// <param name="sql">The SQL statement to execute.</param>
		/// <param name="parameters">Anonymous object providing parameters for the query.</param>
		public dynamic GetRow(string sql, dynamic parameters)
		{
			using (var conn = getConnection())
			using (var cmd = new SqlCommand(sql, conn))
			{
				if (parameters != null)
					AddParametersToCommand(cmd, parameters);

				var reader = cmd.ExecuteReader(CommandBehavior.SingleResult | CommandBehavior.SingleRow);

				if (!reader.Read())
					return null;

				return MapReaderRowToObject(reader);
			}
		}

		/// <summary>
		/// Executes the batch, and returns the first column of the first row of the first result set returned by the query.
		/// Additional columns or rows are ignored.
		/// </summary>
		/// <param name="sql">The SQL statement to execute.</param>
		/// <exception cref="NoRowsException" />
		public T GetScalar<T>(string sql)
		{
			return GetScalar<T>(sql, null);
		}

		/// <summary>
		/// Executes the batch, and returns the first column of the first row of the first result set returned by the query.
		/// Additional columns or rows are ignored.
		/// </summary>
		/// <param name="sql">The SQL statement to execute.</param>
		/// <param name="parameters">Anonymous object providing parameters for the query.</param>
		/// <exception cref="NoRowsException" />
		public T GetScalar<T>(string sql, dynamic parameters)
		{
			using (var conn = getConnection())
			using (var cmd = new SqlCommand(sql, conn))
			{
				if (parameters != null)
					AddParametersToCommand(cmd, parameters);

				object result = cmd.ExecuteScalar();
				
				if (result == null)
					throw new NoRowsException();

				if (result is DBNull)
					return default(T);

				return (T)result;
			}
		}

		/// <summary>
		/// Executes a batch and returns the number of rows affected.
		/// </summary>
		/// <param name="sql">The SQL statement to execute.</param>
		public int Execute(string sql)
		{
			return Execute(sql, null);
		}

		/// <summary>
		/// Executes a batch and returns the number of rows affected.
		/// </summary>
		/// <param name="sql">The SQL statement to execute.</param>
		/// <param name="parameters">Anonymous object providing parameters for the query.</param>
		public int Execute(string sql, dynamic parameters)
		{
			using (var conn = getConnection())
			using (var cmd = new SqlCommand(sql, conn))
			{
				if (parameters != null)
					AddParametersToCommand(cmd, parameters);

				return cmd.ExecuteNonQuery();
			}
		}

		/// <summary>
		/// Maps a single row from a SqlDataReader into a dynamic object.
		/// </summary>
		/// <param name="reader">The SqlDataReader from which the schema & values should be read.</param>
		internal static dynamic MapReaderRowToObject(SqlDataReader reader)
		{
			IDictionary<string, object> obj = new ExpandoObject();

			for (int i = 0; i < reader.VisibleFieldCount; i++)
				obj[reader.GetName(i)] = reader.IsDBNull(i) ? null : reader[i];

			return obj;
		}

		/// <summary>
		/// These are the default mappings between C# datatypes and their equivalent database types. By default, only the
		/// safe types are mapped, e.g. types that are non-ambiguously translated between C# and SQL Server.
		/// </summary>
		private static readonly Dictionary<Type, Func<object, SqlParameter>> typeHandlers = new Dictionary<Type, Func<object, SqlParameter>> {
			{ typeof(short), x => SqlSmallInt.GetParameter((short)x) },
			{ typeof(short?), x => SqlSmallInt.GetParameter((short?)x) },
			{ typeof(byte), x => SqlTinyInt.GetParameter((byte)x) },
			{ typeof(byte?), x => SqlTinyInt.GetParameter((byte?)x) },
			{ typeof(int), x => SqlInt.GetParameter((int)x) },
			{ typeof(int?), x => SqlInt.GetParameter((int?)x) },
			{ typeof(long), x => SqlBigInt.GetParameter((long)x) },
			{ typeof(long?), x => SqlBigInt.GetParameter((long?)x) },
			{ typeof(bool), x => SqlBit.GetParameter((bool)x) },
			{ typeof(bool?), x => SqlBit.GetParameter((bool?)x) },
			{ typeof(Guid), x => SqlUniqueIdentifier.GetParameter((Guid)x) },
			{ typeof(Guid?), x => SqlUniqueIdentifier.GetParameter((Guid?)x) },
			{ typeof(uint), x => SqlBigInt.GetParameter((uint)x) },
			{ typeof(uint?), x => SqlBigInt.GetParameter((uint?)x) },
			{ typeof(ushort), x => SqlInt.GetParameter((ushort)x) },
			{ typeof(ushort?), x => SqlInt.GetParameter((ushort?)x) }
		};

		internal void AddParametersToCommand(SqlCommand cmd, object parameters)
		{
			// For each property in the dynamic parameters object, create a SqlParameter and add it to the SqlCommand
			foreach (PropertyInfo prop in parameters.GetType().GetProperties())
			{
				SqlParameter param;
				object propValue = prop.GetValue(parameters);
				Type propertyType = propValue != null ? propValue.GetType() : prop.PropertyType;
				var sqlType = propValue as ISqlType;
			
				try
				{
					// If it's a SqlType value, let it create the parameter for us. Otherwise, for native CLR types we'll
					// let the type handlers take care of creating the parameter.
					if (sqlType != null)
						param = sqlType.GetParameter();
					else
						param = typeHandlers[propertyType](propValue);
				}
				catch (KeyNotFoundException)
				{
					throw new TypeNotSupportedException(propertyType);
				}

				// Set parameter name
				param.ParameterName = "@" + prop.Name;

				// If it's a null value, convert it to DBNull.value
				if (param.Value == null)
					param.Value = DBNull.Value;

				cmd.Parameters.Add(param);
			}
		}

		/// <summary>
		/// Returns a raw and open SqlConnection for manual use.
		/// </summary>
		public SqlConnection GetOpenConnection()
		{
			return getConnection();
		}

		/// <summary>
		/// Creates and returns an open SqlConnection.
		/// </summary>
		private SqlConnection getConnection()
		{
			var conn = new SqlConnection(ConnectionString);

			try
			{
				conn.Open();
			}
			catch (SqlException)
			{
				// If we fail, clearing the pooled connection may help
				SqlConnection.ClearPool(conn);
				conn.Open();
			}

			return conn;
		}
	}
}