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
    }

    class MySQLQueryableConnection: IQueryableConnection, IDisposable
    {
        private MySqlConnection DB = null;
        private String ConnectionString = "";
        private MySqlCommand CurrentCmd = null;

        public MySQLQueryableConnection(string connectionstr)
        {
            ConnectionString = connectionstr;
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

        public void Dispose()
        {
            DB.Dispose();
            CurrentCmd.Dispose();
        }
    }

    class MSSQLQueryableConnection : IQueryableConnection, IDisposable
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
