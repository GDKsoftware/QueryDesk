using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QueryDesk
{
    public abstract class CExplainableQuery
    {
        protected IQueryableConnection Connection;
        protected StoredQuery Query;
        protected StoredQuery ExplainQuery;

        protected string Error = "";

        public CExplainableQuery(IQueryableConnection connection, StoredQuery query)
        {
            Connection = connection;
            Query = query;

            initExplanation();
        }

        protected abstract void initExplanation();

        public abstract ulong getMaxResults();
        public abstract bool isAllIndexed();
        public abstract bool isUsingBadStuff();

        public bool hasErrors()
        {
            return (Error != "");
        }

        public string getErrorMsg()
        {
            return Error;
        }

        public StoredQuery _get()
        {
            return ExplainQuery;
        }
    }

    public class QueryExplanationFactory
    {
        public static CExplainableQuery newExplain(IQueryableConnection connection, StoredQuery query)
        {
            if (query.SQL.StartsWith("select", StringComparison.OrdinalIgnoreCase)) {
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

    class MySQLQueryExplanation: CExplainableQuery
    {
        protected DataTable Results;

        public MySQLQueryExplanation(IQueryableConnection connection, StoredQuery query) : base(connection, query)
        {
        }

        protected override void initExplanation()
        {
            string sql = "EXPLAIN " + Query.SQL;

            ExplainQuery = new StoredQuery(sql);
            ExplainQuery.CopyParamsFrom(Query);

            if (Connection.Query(ExplainQuery))
            {
                try
                {
                    Results = Connection.ResultsAsDataTable();
                }
                catch (Exception e)
                {
                    Error = e.Message;
                    Results = null;
                }
                
                Connection.CloseQuery();
            }
        }

        /* Explain example:
         
           id	selecttype	table		type	possiblekeys	key			keylen	ref 				rows	extra
            1	SIMPLE		charitem	ref		PRIMARY			PRIMARY		8		const				17		Using where
            1	SIMPLE		item		eq_ref	PRIMARY			PRIMARY		8		lfs.charitem.itemid	1	
         
            "id"	"select_type"	"table"	"type"	"possible_keys"	"key"	"key_len"	"ref"	"rows"	"Extra"
            "1"	"SIMPLE"	"item"	"ALL"	"PRIMARY"	\N	\N	\N	"58910"	""
            "1"	"SIMPLE"	"charitem"	"ref"	"Index_6"	"Index_6"	"4"	"lfs.item.id"	"4"	"Using where"

         */

        public override ulong getMaxResults()
        {
            ulong i = 0;
            if (Results != null)
            {
                i = Results.AsEnumerable().Max(row =>
                        (row["rows"] == null) ? 0 : (ulong)row["rows"]
                    );
            }

            return i;
        }

        public override bool isAllIndexed()
        {
            int i = 0;
            if (Results != null)
            {
                Results.AsEnumerable().Sum(row =>
                    (row["key"] is System.DBNull) ? 0 : 1
                );

                return (i == Results.Rows.Count);
            }

            return true;
        }

        public override bool isUsingBadStuff()
        {
            if (Results != null)
            {
                foreach (var row in Results.AsEnumerable())
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

    class MSSQLQueryExplanation: CExplainableQuery
    {
        public MSSQLQueryExplanation(IQueryableConnection connection, StoredQuery query)
            : base(connection, query)
        {
        }

        protected override void initExplanation()
        {
            throw new NotImplementedException();
        }

        public override ulong getMaxResults()
        {
            throw new NotImplementedException();
        }

        public override bool isAllIndexed()
        {
            throw new NotImplementedException();
        }

        public override bool isUsingBadStuff()
        {
            throw new NotImplementedException();
        }
    }
}
