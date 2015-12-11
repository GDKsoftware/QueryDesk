using System;
using System.Data;
using System.Linq;

namespace QueryDesk
{
    public abstract class CExplainableQuery
    {
        protected IQueryableConnection connection;
        protected StoredQuery query;
        protected StoredQuery explainQuery;

        protected string error = string.Empty;

        public CExplainableQuery(IQueryableConnection connection, StoredQuery query)
        {
            this.connection = connection;
            this.query = query;

            InitExplanation();
        }

        protected abstract void InitExplanation();

        public abstract ulong GetMaxResults();
        
        public abstract bool IsAllIndexed();

        public abstract bool IsUsingBadStuff();

        public bool HasErrors()
        {
            return (error != string.Empty);
        }

        public string GetErrorMsg()
        {
            return error;
        }

        public StoredQuery _get()
        {
            return explainQuery;
        }
    }

    public class QueryExplanationFactory
    {
        public static CExplainableQuery NewExplain(IQueryableConnection connection, StoredQuery query)
        {
            if (query.SQL.StartsWith("select", StringComparison.OrdinalIgnoreCase))
            {
                if (connection is MySQLQueryableConnection)
                {
                    return new MySQLQueryExplanation(connection, query);
                }
                else if (connection is MSSQLQueryableConnection)
                {
                    return null; // new MSSQLQueryExplanation(connection, query);
                }
            }

            return null;
        }
    }

    public class MySQLQueryExplanation : CExplainableQuery
    {
        protected DataTable results;

        public MySQLQueryExplanation(IQueryableConnection connection, StoredQuery query) : base(connection, query)
        {
        }

        protected override void InitExplanation()
        {
            string sql = "EXPLAIN " + query.SQL;

            explainQuery = new StoredQuery(sql);
            explainQuery.CopyParamsFrom(query);

            if (connection.Query(explainQuery))
            {
                try
                {
                    results = connection.ResultsAsDataTable();
                }
                catch (Exception e)
                {
                    error = e.Message;
                    results = null;
                }
                
                connection.CloseQuery();
            }
        }

        /* Explain example:
         
           id   selecttype  table       type    possiblekeys    key         keylen  ref                 rows    extra
            1   SIMPLE      charitem    ref     PRIMARY         PRIMARY     8       const               17      Using where
            1   SIMPLE      item        eq_ref  PRIMARY         PRIMARY     8       lfs.charitem.itemid 1   
         
            "id"    "select_type"   "table" "type"  "possible_keys" "key"   "key_len"   "ref"   "rows"  "Extra"
            "1" "SIMPLE"    "item"  "ALL"   "PRIMARY"   \N  \N  \N  "58910" ""
            "1" "SIMPLE"    "charitem"  "ref"   "Index_6"   "Index_6"   "4" "lfs.item.id"   "4" "Using where"

         */

        public override ulong GetMaxResults()
        {
            ulong i = 0;
            if (results != null)
            {
                i = results.AsEnumerable().Max(row =>
                        (row["rows"] == null) ? 0 : (ulong)row["rows"]);
            }

            return i;
        }

        public override bool IsAllIndexed()
        {
            int i = 0;
            if (results != null)
            {
                results.AsEnumerable().Sum(row =>
                    (row["key"] is System.DBNull) ? 0 : 1);

                return (i == results.Rows.Count);
            }

            return true;
        }

        public override bool IsUsingBadStuff()
        {
            if (results != null)
            {
                foreach (var row in results.AsEnumerable())
                {
                    if (!(row["extra"] is System.DBNull))
                    {
                        string s = (string)row["extra"];
                        if (s.Contains("filesort"))
                        {
                            return true;
                        }
                        else if (s.Contains("temporary"))
                        {
                            return true;
                        }
                    }
                }
            }

            return false;
        }
    }

    public class MSSQLQueryExplanation : CExplainableQuery
    {
        public MSSQLQueryExplanation(IQueryableConnection connection, StoredQuery query)
            : base(connection, query)
        {
        }

        protected override void InitExplanation()
        {
            throw new NotImplementedException();
        }

        public override ulong GetMaxResults()
        {
            throw new NotImplementedException();
        }

        public override bool IsAllIndexed()
        {
            throw new NotImplementedException();
        }

        public override bool IsUsingBadStuff()
        {
            throw new NotImplementedException();
        }
    }
}
