using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.SqlClient;
using System.Data;

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
        List<string> ListFieldNames(string tablename);
    }

    public class MySQLQueryableConnection: IQueryableConnection, IDisposable
    {
        private MySqlConnection DB = null;
        private String ConnectionString = "";
        private MySqlCommand CurrentCmd = null;

        public MySQLQueryableConnection(string connectionstr)
        {
            ConnectionString = connectionstr;

            ConnectionString = ConnectionString + ";Allow Zero Datetime=True";
        }

        public bool Connect()
        {
            try
            {
                DB = new MySqlConnection(ConnectionString);

                DB.Open(); // throws exception if failed to connect

                return true;
            }
            catch (MySql.Data.MySqlClient.MySqlException)
            {
                throw;
            }
        }

        public void Disconnect()
        {
            DB.Close();
        }


        public bool Query(StoredQuery qry)
        {
            CurrentCmd = new MySqlCommand(qry.SQL, DB);
            foreach (var p in qry.parameters)
            {
                CurrentCmd.Parameters.AddWithValue(p.Key, p.Value);
            }


            return true;
        }


        public DataTable ResultsAsDataTable()
        {
            MySqlDataAdapter adapter = new MySqlDataAdapter();
            adapter.SelectCommand = CurrentCmd;

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

        public void CloseQuery()
        {
            if (CurrentCmd != null)
            {
                CurrentCmd.Dispose();
                CurrentCmd = null;
            }
        }

        public void Dispose()
        {
            DB.Dispose();
            if (CurrentCmd != null)
            {
                CurrentCmd.Dispose();
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

        public List<string> ListFieldNames(string tablename)
        {
            var tblqry = new StoredQuery("DESC `" + tablename + "`;");
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
    }

    public class MSSQLQueryableConnection : IQueryableConnection, IDisposable
    {
        private SqlConnection DB = null;
        private string ConnectionString = "";
        private SqlCommand CurrentCmd = null;

        public MSSQLQueryableConnection(string connectionstr)
        {
            ConnectionString = connectionstr;
        }

        public bool Connect()
        {
            DB = new SqlConnection(ConnectionString);
            DB.Open();

            return true;
        }

        public void Disconnect()
        {
            DB.Close();
        }

        public bool Query(StoredQuery qry)
        {
            CurrentCmd = new SqlCommand(qry.SQL, DB);
            foreach (var p in qry.parameters)
            {
                CurrentCmd.Parameters.AddWithValue(p.Key, p.Value);
            }

            return true;
        }

        public void CloseQuery()
        {
            if (CurrentCmd != null)
            {
                CurrentCmd.Dispose();
                CurrentCmd = null;
            }
        }

        public DataTable ResultsAsDataTable()
        {
            SqlDataAdapter adapter = new SqlDataAdapter();
            adapter.SelectCommand = CurrentCmd;

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
            DB.Dispose();
            CurrentCmd.Dispose();
        }


        public List<string> ListTableNames()
        {
            var tblqry = new StoredQuery("SELECT TABLE_NAME FROM information_schema.tables;");
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

        public List<string> ListFieldNames(string tablename)
        {
            var tblqry = new StoredQuery("SELECT COLUMN_NAME" +
                " FROM INFORMATION_SCHEMA.COLUMNS" +
                " WHERE TABLE_NAME = 'locationdevices';");
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


            return null;
        }
    }
}
