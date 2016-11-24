using System;
using System.Collections.Generic;
using System.Linq;
using System.Data;
using System.Data.SqlClient;

namespace QueryDesk
{
    public class MSSQLQueryableConnection : IQueryableConnection, IDisposable
    {
        private SqlConnection db = null;
        private string connectionString = string.Empty;
        private SqlCommand currentCmd = null;

        public MSSQLQueryableConnection(string connectionstr)
        {
            connectionString = connectionstr;
        }

        public bool Connect()
        {
            db = new SqlConnection(connectionString);
            db.Open();

            return true;
        }

        public void Disconnect()
        {
            db.Close();
        }

        public bool Query(StoredQuery qry)
        {
            currentCmd = new SqlCommand(qry.SQL, db);
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
            SqlDataAdapter adapter = new SqlDataAdapter();
            adapter.SelectCommand = currentCmd;

            DataSet ds = new DataSet();
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
            var tblqry = new StoredQuery("SELECT TABLE_NAME FROM INFORMATION_SCHEMA.TABLES;");
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
            var tblqry = new StoredQuery(
                "SELECT COLUMN_NAME, DATA_TYPE " +
                " FROM INFORMATION_SCHEMA.COLUMNS " +
                " WHERE TABLE_NAME = '" + tablename + "';");
            if (Query(tblqry))
            {
                var dt = ResultsAsDataTable();
                if (dt != null)
                {
                    var lst = new Dictionary<string, string>();
                    foreach (var row in dt.AsEnumerable())
                    {
                        lst.Add(row.Field<string>(0), row.Field<string>(1));
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
