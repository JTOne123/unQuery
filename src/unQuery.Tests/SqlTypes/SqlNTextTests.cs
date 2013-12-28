﻿using NUnit.Framework;
using System;
using System.Data;
using unQuery.SqlTypes;

namespace unQuery.Tests.SqlTypes
{
	public class SqlNTextTests : TestFixture
	{
		[Test]
		public void GetTypeHandler()
		{
			Assert.IsInstanceOf<ITypeHandler>(SqlNText.GetTypeHandler());
		}

		[Test]
		public void CreateParamFromValue()
		{
			Assert.Throws<TypeCannotBeUsedAsAClrTypeException>(() => SqlNText.GetTypeHandler().CreateParamFromValue(null));
		}

		[Test]
		public void CreateMetaData()
		{
			Assert.Throws<TypeCannotBeUsedAsAClrTypeException>(() => SqlNText.GetTypeHandler().CreateMetaData(null));

			ITypeHandler col = new SqlNText("ру́сский");
			var meta = col.CreateMetaData("Test");
			Assert.AreEqual(SqlDbType.NText, meta.SqlDbType);
			Assert.AreEqual(-1, meta.MaxLength);
			Assert.AreEqual("Test", meta.Name);
		}

		[Test]
		public void GetParameter()
		{
			ISqlType type = new SqlNText("ру́сский");
			TestHelper.AssertSqlParameter(type.GetParameter(), SqlDbType.NText, "ру́сский", size: -1);

			type = new SqlNText(null);
			TestHelper.AssertSqlParameter(type.GetParameter(), SqlDbType.NText, DBNull.Value, size: -1);
		}

		[Test]
		public void GetRawValue()
		{
			ISqlType type = new SqlNText("ру́сский");
			Assert.AreEqual("ру́сский", type.GetRawValue());

			type = new SqlNText(null);
			Assert.Null(type.GetRawValue());
		}

		[Test]
		public void Factory()
		{
			Assert.IsInstanceOf<SqlNText>(Col.NText("ру́сский"));
		}

		[Test]
		public void Structured()
		{
			var rows = DB.GetRows("SELECT * FROM @Input", new {
				Input = Col.Structured("ListOfNTexts", new[] {
					new { A = Col.NText("язк") },
					new { A = Col.NText(null) }
				})
			});

			Assert.AreEqual(2, rows.Count);
			Assert.AreEqual(typeof(string), rows[0].A.GetType());
			Assert.AreEqual("язк", rows[0].A);
			Assert.AreEqual(null, rows[1].A);
		}

		[Test]
		public void TypeMaps()
		{
			Assert.IsInstanceOf<ITypeHandler>(unQueryDB.ClrTypeHandlers[typeof(SqlNText)]);
			Assert.IsInstanceOf<ITypeHandler>(unQueryDB.SqlDbTypeHandlers[SqlDbType.NText]);
		}
	}
}