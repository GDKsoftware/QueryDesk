using System;
using System.Collections.Generic;
using System.Linq;
using System.Data.SqlClient;
using System.Data;
using MySql.Data.MySqlClient;
using System.Data.SQLite;

namespace QueryDesk
{
    public interface IQueryableConnection
    {
        bool Connect();

        void Disconnect();

        bool Query(StoredQuery qry);

        DataTable ResultsAsDataTable();

        void CloseQuery();

        List<string> ListTableNames();

        Dictionary<string, string> ListFieldNames(string tablename);

        char GetParamPrefixChar();
    }

    public static class ConnectionFactory
    {
        public static IQueryableConnection NewConnection(int type, string connectionstring)
        {
            if (type == 1)
            {
                return new MySQLQueryableConnection(connectionstring);
            }
            else if (type == 2)
            {
                return new MSSQLQueryableConnection(connectionstring);
            }
            else if (type == 3)
            {
                return new SQLiteQueryableConnection(connectionstring);
            }

            return null;
        }
    }

    public class MySQLQueryableConnection : IQueryableConnection, IDisposable
    {
        private MySqlConnection db = null;
        private string connectionString = string.Empty;
        private MySqlCommand currentCmd = null;

        public MySQLQueryableConnection(string connectionstr)
        {
            connectionString = connectionstr;

            connectionString = connectionString + ";Allow Zero Datetime=True";
        }

        public bool Connect()
        {
            try
            {
                db = new MySqlConnection(connectionString);

                db.Open(); // throws exception if failed to connect

                return true;
            }
            catch (MySql.Data.MySqlClient.MySqlException)
            {
                throw;
            }
        }

        public void Disconnect()
        {
            db.Close();
        }

        public bool Query(StoredQuery qry)
        {
            currentCmd = new MySqlCommand(qry.SQL, db);
            foreach (var p in qry.Parameters)
            {
                currentCmd.Parameters.AddWithValue(p.Key, p.Value);
            }

            return true;
        }

        public DataTable ResultsAsDataTable()
        {
            MySqlDataAdapter adapter = new MySqlDataAdapter();
            adapter.SelectCommand = currentCmd;

            DataSet ds = new DataSet();
            try
            {
                adapter.Fill(ds, "query");
            }
            catch (Exception e)
            {
                throw e;
            }

            return ds.Tables["query"];
        }

        public void CloseQuery()
        {
            if (currentCmd != null)
            {
                currentCmd.Dispose();
                currentCmd = null;
            }
        }

        public void Dispose()
        {
            db.Dispose();
            if (currentCmd != null)
            {
                currentCmd.Dispose();
            }
        }

        public List<string> ListTableNames()
        {
            var tblqry = new StoredQuery("SHOW TABLES;");
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
            var tblqry = new StoredQuery("DESC `" + tablename + "`;");
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
            return '?';
        }
    }


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
