﻿using System.Data;
using System.Data.SqlClient;

namespace unQuery.SqlTypes
{
	public class SqlInt : ISqlType
	{
		private readonly int? value;

		public SqlInt(int? value)
		{
			this.value = value;
		}

		public static explicit operator SqlInt(long? value)
		{
			return new SqlInt((int?)value);
		}

		public SqlParameter GetParameter()
		{
			return GetParameter(value);
		}

		public SqlDbType GetDbType()
		{
			return SqlDbType.Int;
		}

		public object GetRawValue()
		{
			return value;
		}

		public static SqlParameter GetParameter(int? value)
		{
			return new SqlParameter {
				SqlDbType = SqlDbType.Int,
				Value = TypeHelper.GetDBNullableValue(value)
			};
		}
	}
}