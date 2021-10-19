using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;
using System.Linq;
using System.Text.RegularExpressions;

namespace NPoco.DatabaseTypes
{
    public class OleDbDatabaseType : DatabaseType
    {
        public bool UseOutputClause { get; set; }

        private static readonly Regex OrderByAlias = new Regex(@"[\""\[\]\w]+\.([\[\]\""\w]+)", RegexOptions.Compiled | RegexOptions.Multiline | RegexOptions.Singleline | RegexOptions.IgnoreCase);

        public override bool UseColumnAliases()
        {
            return true;
        }

        public override string BuildPageQuery(long skip, long take, PagingHelper.SQLParts parts, ref object[] args, string primaryKey = "")
        {
            if (string.IsNullOrEmpty(primaryKey))
            {
                primaryKey = "id";
            }
            string sqlPage = null;
            if (string.IsNullOrEmpty(parts.sqlOrderBy))
            {
                sqlPage = "select TOP " + take + " " + parts.sqlSelectRemoved;
            }
            else
            {
                parts.sqlOrderBy = OrderByAlias.Replace(parts.sqlOrderBy, "$1");
                sqlPage = "select TOP " + take + " " + parts.sqlSelectRemoved.Replace(parts.sqlOrderBy, "");
            }
            if (skip > 0)
            {
                if (sqlPage.ToLower().Contains("where"))
                {
                    sqlPage += ("and " + primaryKey + " not in (select TOP " + skip + " " + parts.sqlSelectRemoved.Replace(parts.sqlColumns, primaryKey) + ")");
                }
                else
                {
                    sqlPage += (" where " + primaryKey + " not in (select TOP " + skip + " " + parts.sqlSelectRemoved.Replace(parts.sqlColumns, primaryKey) + ")");
                }
            }
            if (!string.IsNullOrEmpty(parts.sqlOrderBy))
            {
                sqlPage += (" " + parts.sqlOrderBy);
            }

            return sqlPage;
        }
        
        private void AdjustSqlInsertCommandText(DbCommand cmd, bool useOutputClause)
        {
            if (!UseOutputClause && !useOutputClause)
            {
                //cmd.CommandText += ";SELECT SCOPE_IDENTITY();";//Deleted
            }
        }
        
        public override string GetInsertOutputClause(string primaryKeyName, bool useOutputClause)
        {
            if (UseOutputClause || useOutputClause)
            {
                return string.Format(" OUTPUT INSERTED.{0}", EscapeSqlIdentifier(primaryKeyName));
            }
            return base.GetInsertOutputClause(primaryKeyName, useOutputClause);
        }

        public override string GetDefaultInsertSql(string tableName, string primaryKeyName, bool useOutputClause, string[] names, string[] parameters)
        {
            return string.Format("INSERT INTO {0}{1} DEFAULT VALUES", EscapeTableName(tableName), GetInsertOutputClause(primaryKeyName, useOutputClause));
        }

        public override object ExecuteInsert<T>(Database db, DbCommand cmd, string primaryKeyName, bool useOutputClause, T poco, object[] args)
        {
            AdjustSqlInsertCommandText(cmd, useOutputClause);
            return db.ExecuteScalarHelper(cmd);
        }

#if !NET35 && !NET40
        public override System.Threading.Tasks.Task<object> ExecuteInsertAsync<T>(Database db, DbCommand cmd, string primaryKeyName, bool useOutputClause, T poco, object[] args)
        {
            AdjustSqlInsertCommandText(cmd, useOutputClause);
            return db.ExecuteScalarHelperAsync(cmd);
        }
#endif

        public override string GetExistsSql()
        {
            return "IF EXISTS (SELECT 1 FROM {0} WHERE {1}) SELECT 1 ELSE SELECT 0";
        }

#if !DNXCORE50
        public override void InsertBulk<T>(IDatabase db, IEnumerable<T> pocos)
        {
            SqlBulkCopyHelper.BulkInsert(db, pocos);
        }
#endif

        public override IsolationLevel GetDefaultTransactionIsolationLevel()
        {
            return IsolationLevel.ReadCommitted;
        }

        public override DbType? LookupDbType(Type type, string name)
        {
            if (type == typeof(TimeSpan) || type == typeof(TimeSpan?))
                return null;

            return base.LookupDbType(type, name);
        }

        public override string GetProviderName()
        {
            return "System.Data.OleDb";
        }

        public override object ProcessDefaultMappings(PocoColumn pocoColumn, object value)
        {
            if (pocoColumn.MemberInfoData.MemberType == typeof (byte[]) && value == null)
            {
                return new SqlParameter("__bytes", SqlDbType.VarBinary, -1) { Value = DBNull.Value };
            }
            return base.ProcessDefaultMappings(pocoColumn, value);
        }
    }
}