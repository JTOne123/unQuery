﻿using System.Data;
using System.Data.SqlClient;

namespace unQuery.SqlTypes
{
	// TODO: Better handling of length. Instead of setting explicit length, try to reuse common lengths.
	public class SqlVarchar : ISqlType
	{
		private readonly string value;
		private readonly int? size;

		public SqlVarchar(string value)
		{
			this.value = value;
		}

		public SqlVarchar(string value, int size)
		{
			this.value = value;
			this.size = size;
		}

		public static explicit operator SqlVarchar(string value)
		{
			return new SqlVarchar(value);
		}

		public SqlParameter GetParameter()
		{
			return GetParameter(value, size);
		}

		public static SqlParameter GetParameter(object value)
		{
			return new SqlParameter {
				SqlDbType = SqlDbType.VarChar,
				Value = value,
				Size = value.ToString().Length
			};
		}

		public static SqlParameter GetParameter(object value, int? size)
		{
			return new SqlParameter
			{
				SqlDbType = SqlDbType.VarChar,
				Value = value,
				Size = size ?? value.ToString().Length
			};
		}
	}
}