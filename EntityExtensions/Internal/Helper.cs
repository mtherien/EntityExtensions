using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Data.Entity.Core.Metadata.Edm;
using System.Linq;
using System.Reflection;
using EntityExtensions.Common;

namespace EntityExtensions.Internal
{
    internal static class Helper
    {
        internal static bool IsNumeric(this object o)
        {
            switch (Type.GetTypeCode(o.GetType()))
            {
                case TypeCode.Byte:
                case TypeCode.SByte:
                case TypeCode.UInt16:
                case TypeCode.UInt32:
                case TypeCode.UInt64:
                case TypeCode.Int16:
                case TypeCode.Int32:
                case TypeCode.Int64:
                case TypeCode.Decimal:
                case TypeCode.Double:
                case TypeCode.Single:
                    return true;
                default:
                    return false;
            }
        }

        internal static string GetSqlServerType(Type type, EdmProperty typeUsage)
        {
            if (type.IsGenericType)
            {
                //This is nullable
                type = type.GetGenericArguments()[0];
            }

            if (typeUsage?.PrimitiveType == null ||
                !typeUsage.PrimitiveType.NamespaceName.Equals("SqlServer", StringComparison.OrdinalIgnoreCase))
            {
                switch (Type.GetTypeCode(type))
                {
                    case TypeCode.Int16:
                        return "smallint";
                    case TypeCode.Int32:
                        return "int";
                    case TypeCode.Decimal:
                        return "decimal";
                    case TypeCode.DateTime:
                        return "datetime";
                    case TypeCode.String:
                        return "nvarchar(MAX)";
                    case TypeCode.Boolean:
                        return "bit";
                    case TypeCode.Int64:
                        return "bigint";
                    case TypeCode.Double:
                        return "float";
                    default:
                        if (type == typeof(Guid))
                        {
                            return "uniqueidentifier";
                        }
                        else if (type == typeof(byte[]))
                        {
                            return "binary";
                        }
                        else
                        {
                            throw new Exception($"Unsupported database column type: {type.Name}");
                        }
                }
            }

            var sqlTypeName = typeUsage.PrimitiveType.Name;
            switch (sqlTypeName.ToLower())
            {
                case "decimal":
                    if (typeUsage.Precision != null && typeUsage.Scale != null)
                    {
                        return $"decimal({typeUsage.Precision},{typeUsage.Scale})";
                    }
                    else
                    {
                        return "decimal";
                    }
                case "nvarchar":
                    if (typeUsage.MaxLength != null)
                    {
                        return $"nvarchar({typeUsage.MaxLength})";
                    }
                    else
                    {
                        return "varchar(MAX)";
                    }
                case "varchar":
                    if (typeUsage.MaxLength != null)
                    {
                        return $"varchar({typeUsage.MaxLength})";
                    }
                    else
                    {
                        return "varchar(MAX)";
                    }
                case "char":
                    if (typeUsage.MaxLength != null)
                    {
                        return $"char({typeUsage.MaxLength})";
                    }
                    else
                    {
                        return "char";
                    }
                case "nchar":
                    if (typeUsage.MaxLength != null)
                    {
                        return $"nchar({typeUsage.MaxLength})";
                    }
                    else
                    {
                        return "nchar";
                    }
                default:
                    return sqlTypeName;
            }
        }
        /// <summary>
        /// Converts a list of entities to a DataTable object.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="context"></param>
        /// <param name="entities"></param>
        /// <param name="tabCols"></param>
        /// <returns></returns>
        internal static DataTable GetDatatable<T>(this DbContext context, ICollection<T> entities, IDictionary<string, EntityColumnInformation> tabCols = null)
        {
            if (tabCols == null) tabCols = context.GetTableColumns<T>();
            var table = new DataTable();
            foreach (var colName in tabCols.Keys)
            {
                var colType = tabCols[colName].Type.IsGenericType
                    ? tabCols[colName].Type.GetGenericArguments()[0]
                    : tabCols[colName].Type;
                table.Columns.Add(colName, colType);
            }

            table.BeginLoadData();
            foreach (var entity in entities)
            {
                var row = table.NewRow();
                foreach (DataColumn column in table.Columns)
                {
                    row[column] = tabCols[column.ColumnName].PropertyInfo?.GetValue(entity, null) ?? DBNull.Value;
                    if (row[column] == DBNull.Value &&
                        tabCols[column.ColumnName].HasDiscriminator)
                    {
                        row[column] = tabCols[column.ColumnName].DiscriminatorValue ?? DBNull.Value;
                    }
                }
                table.Rows.Add(row);
            }
            table.EndLoadData();
            return table;
        }

    }
}
