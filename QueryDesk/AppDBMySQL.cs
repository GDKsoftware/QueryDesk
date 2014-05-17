using MySql.Data.MySqlClient;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QueryDesk
{
    class AppDBMySQL: IAppDBServersAndQueries, IAppDBEditableServers
    {
        private string connectionstring;
        private MySqlConnection DB;

        public AppDBMySQL(string connectionstring)
        {
            this.connectionstring = connectionstring;

            try
            {
                DB = new MySqlConnection(connectionstring);

                DB.Open(); // throws exception if failed to connect
            }
            catch (MySql.Data.MySqlClient.MySqlException)
            {
                // todo: make unique exception that everyone AppDB class can use
                throw;
            }
        }

        public IEnumerable getServerListing()
        {
            MySqlDataAdapter adapter = new MySqlDataAdapter();
            var cmd = new MySqlCommand("select id, name, host, port, username, password, databasename, extraparams from connection order by name asc", DB);

            adapter.SelectCommand = cmd;

            DataSet ds = new DataSet();
            adapter.Fill(ds, "connection");

            var dt = ds.Tables["connection"];

            return dt.DefaultView;
        }

        public IEnumerable getQueriesListing(long server_id)
        {
            MySqlDataAdapter adapter = new MySqlDataAdapter();
            var cmd = new MySqlCommand("select id, name, sqltext from query where connection_id=?connection_id order by name asc", DB);
            cmd.Parameters.AddWithValue("?connection_id", server_id);

            adapter.SelectCommand = cmd;

            DataSet ds = new DataSet();
            adapter.Fill(ds, "query");

            var dt = ds.Tables["query"];
            return dt.DefaultView;
        }


        public long saveServer(AppDBServerLink server)
        {
            MySqlCommand qry;

            if (server.id > 0)
            {
                // update
                qry = new MySqlCommand("update connection set name=?name, host=?host, port=?port, username=?username, password=?password, databasename=?databasename, extraparams=?extraparams where id=?id", DB);
                qry.Parameters.AddWithValue("?id", server.id);
            }
            else
            {
                // insert
                qry = new MySqlCommand("insert into connection ( name, host, port, username, password, databasename, extraparams) values (?name,?host,?port,?username,?password,?databasename,?extraparams); select last_insert_id();", DB);
            }

            qry.Parameters.AddWithValue("?name", server.name);
            qry.Parameters.AddWithValue("?host", server.host);
            qry.Parameters.AddWithValue("?port", server.port);
            qry.Parameters.AddWithValue("?username", server.username);
            qry.Parameters.AddWithValue("?password", server.password);
            qry.Parameters.AddWithValue("?databasename", server.databasename);
            qry.Parameters.AddWithValue("?extraparams", server.extraparams);

            qry.ExecuteNonQuery();

            // get inserted id and assign to in server.id
            if (server.id <= 0)
            {
                server.id = qry.LastInsertedId;
            }

            return server.id;
        }

        public void delServer(AppDBServerLink server)
        {
            MySqlCommand qry;

            if (server.id > 0)
            {
                qry = new MySqlCommand("delete from connection where id=?id", DB);
                qry.Parameters.AddWithValue("?id", server.id);

                qry.ExecuteNonQuery();

                server.id = 0;
            }
            else
            {
                // if this server isn't an entry in the database, that's ok with me
                //throw new Exception("");
            }
        }
    }
}
