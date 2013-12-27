﻿using NUnit.Framework;
using System;
using System.Data;
using unQuery.SqlTypes;

namespace unQuery.Tests.SqlTypes
{
	public class SqlDateTests : TestFixture
	{
		private readonly DateTime testDate = new DateTime(2013, 05, 12);

		[Test]
		public void GetTypeHandler()
		{
			Assert.IsInstanceOf<ITypeHandler>(SqlDate.GetTypeHandler());
		}

		[Test]
		public void CreateParamFromValue()
		{
			Assert.Throws<TypeCannotBeUsedAsAClrTypeException>(() => SqlChar.GetTypeHandler().CreateParamFromValue(null));
		}

		[Test]
		public void CreateMetaData()
		{
			Assert.Throws<TypeCannotBeUsedAsAClrTypeException>(() => SqlDate.GetTypeHandler().CreateMetaData(null));

			ITypeHandler col = new SqlDate(testDate);
			var meta = col.CreateMetaData("Test");
			Assert.AreEqual(SqlDbType.Date, meta.SqlDbType);
			Assert.AreEqual("Test", meta.Name);
		}

		[Test]
		public void GetParameter()
		{
			ISqlType type = new SqlDate(testDate);
			TestHelper.AssertSqlParameter(type.GetParameter(), SqlDbType.Date, testDate);

			type = new SqlDate(null);
			TestHelper.AssertSqlParameter(type.GetParameter(), SqlDbType.Date, DBNull.Value);
		}

		[Test]
		public void GetRawValue()
		{
			ISqlType type = new SqlDate(testDate);
			Assert.AreEqual(testDate, type.GetRawValue());

			type = new SqlDate(null);
			Assert.Null(type.GetRawValue());
		}

		[Test]
		public void Factory()
		{
			Assert.IsInstanceOf<SqlDate>(Col.Date(testDate));
		}

		[Test]
		public void Structured()
		{
			var rows = DB.GetRows("SELECT * FROM @Input", new {
				Input = Col.Structured("ListOfDates", new[] {
					new { A = Col.Date(testDate) },
					new { A = Col.Date(null) }
				})
			});

			Assert.AreEqual(2, rows.Count);
			Assert.AreEqual(typeof(DateTime), rows[0].A.GetType());
			Assert.AreEqual(testDate, rows[0].A);
			Assert.AreEqual(null, rows[1].A);
		}

		[Test]
		public void TypeMaps()
		{
			Assert.IsInstanceOf<ITypeHandler>(unQueryDB.ClrTypeHandlers[typeof(SqlDate)]);
			Assert.IsInstanceOf<ITypeHandler>(unQueryDB.SqlDbTypeHandlers[SqlDbType.Date]);
		}
	}
}