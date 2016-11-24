using System;
using System.Collections.Generic;
using System.Linq;
using System.Data;
using System.Data.SQLite;

namespace QueryDesk
{
    public class SQLiteQueryableConnection : IQueryableConnection, IDisposable
    {
        private SQLiteConnection db;
        private string connectionString = string.Empty;
        private SQLiteCommand currentCmd = null;

        public SQLiteQueryableConnection(string connectionstr)
        {
            connectionString = connectionstr;
        }

        public bool Connect()
        {
            db = new SQLiteConnection(connectionString);
            db.Open();

            return true;
        }

        public void Disconnect()
        {
            db.Close();
        }

        public bool Query(StoredQuery qry)
        {
            currentCmd = new SQLiteCommand(qry.SQL, db);
            foreach (var p in qry.Parameters)
            {
                currentCmd.Parameters.AddWithValue(p.Key, p.Value);
            }

            return true;
        }

        public void CloseQuery()
        {
            if (currentCmd != null)
            {
                currentCmd.Dispose();
                currentCmd = null;
            }
        }

        public DataTable ResultsAsDataTable()
        {
            var adapter = new SQLiteDataAdapter();
            adapter.SelectCommand = currentCmd;

            var ds = new DataSet();
            try
            {
                adapter.Fill(ds, "query");
            }
            catch (Exception)
            {
                throw;
            }

            return ds.Tables["query"];
        }

        public void Dispose()
        {
            db.Dispose();
            currentCmd.Dispose();
        }

        public List<string> ListTableNames()
        {
            var tblqry = new StoredQuery("SELECT name FROM sqlite_master WHERE type='table';");
            if (Query(tblqry))
            {
                var dt = ResultsAsDataTable();
                if (dt != null)
                {
                    var lst = new List<string>();
                    foreach (var row in dt.AsEnumerable())
                    {
                        lst.Add(row.Field<string>(0));
                    }

                    return lst;
                }

                CloseQuery();
            }

            return null;
        }

        public Dictionary<string, string> ListFieldNames(string tablename)
        {
            var tblqry = new StoredQuery("pragma table_info(" + tablename + ");");
            if (Query(tblqry))
            {
                var dt = ResultsAsDataTable();
                if (dt != null)
                {
                    var lst = new Dictionary<string, string>();
                    foreach (var row in dt.AsEnumerable())
                    {
                        lst.Add(row.Field<string>(1), row.Field<string>(2));
                    }

                    return lst;
                }

                CloseQuery();
            }

            return null;
        }

        public char GetParamPrefixChar()
        {
            return '@';
        }
    }
}
