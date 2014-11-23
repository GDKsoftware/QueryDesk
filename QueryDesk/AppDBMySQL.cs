using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MySql.Data.MySqlClient;

namespace QueryDesk
{
    public class AppDBMySQL : IAppDBServersAndQueries, IAppDBEditableServers, IAppDBEditableQueries, IDisposable
    {
        private string connectionstring;
        private MySqlConnection db;

        public AppDBMySQL(string connectionstring)
        {
            this.connectionstring = connectionstring;

            try
            {
                db = new MySqlConnection(connectionstring);

                db.Open(); // throws exception if failed to connect

                Upgrade();
            }
            catch (MySql.Data.MySqlClient.MySqlException)
            {
                // todo: make unique exception that everyone AppDB class can use
                throw;
            }
        }

        private void Upgrade()
        {
            MySqlDataAdapter adapter = new MySqlDataAdapter();
            var cmd = new MySqlCommand("show columns from connection where Field=?field", db);
            cmd.Parameters.AddWithValue("?field", "type");

            adapter.SelectCommand = cmd;

            DataSet ds = new DataSet();
            adapter.Fill(ds, "connection");

            var dt = ds.Tables["connection"];

            if (dt.Rows.Count == 0)
            {
                var qry = new MySqlCommand("ALTER TABLE  `connection` ADD  `type` INT NOT NULL DEFAULT  '1' AFTER  `id`", db);
                qry.ExecuteNonQuery();
            }
        }

        public IEnumerable GetServerListing()
        {
            MySqlDataAdapter adapter = new MySqlDataAdapter();
            var cmd = new MySqlCommand("select id, type, name, host, port, username, password, databasename, extraparams from connection order by name asc", db);

            adapter.SelectCommand = cmd;

            DataSet ds = new DataSet();
            adapter.Fill(ds, "connection");

            var dt = ds.Tables["connection"];

            return dt.DefaultView;
        }

        public IEnumerable GetQueriesListing(long server_id)
        {
            MySqlDataAdapter adapter = new MySqlDataAdapter();
            var cmd = new MySqlCommand("select id, connection_id, name, sqltext from query where connection_id=?connection_id order by name asc", db);
            cmd.Parameters.AddWithValue("?connection_id", server_id);

            adapter.SelectCommand = cmd;

            DataSet ds = new DataSet();
            adapter.Fill(ds, "query");

            var dt = ds.Tables["query"];
            return dt.DefaultView;
        }

        public long SaveServer(AppDBServerLink server)
        {
            MySqlCommand qry;

            if (server.id > 0)
            {
                // update
                qry = new MySqlCommand("update connection set type=?type, name=?name, host=?host, port=?port, username=?username, password=?password, databasename=?databasename, extraparams=?extraparams where id=?id", db);
                qry.Parameters.AddWithValue("?id", server.id);
            }
            else
            {
                // insert
                qry = new MySqlCommand("insert into connection ( type, name, host, port, username, password, databasename, extraparams) values (?type,?name,?host,?port,?username,?password,?databasename,?extraparams); select last_insert_id();", db);
            }

            qry.Parameters.AddWithValue("?type", server.type);
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
                server.id = (int)qry.LastInsertedId;
            }

            return server.id;
        }

        public void DelServer(AppDBServerLink server)
        {
            MySqlCommand qry;

            if (server.id > 0)
            {
                qry = new MySqlCommand("delete from connection where id=?id", db);
                qry.Parameters.AddWithValue("?id", server.id);

                qry.ExecuteNonQuery();

                server.id = 0;
            }
            else
            {
                // if this server isn't an entry in the database, that's ok with me
                // throw new Exception("");
            }
        }

        public long SaveQuery(AppDBQueryLink query)
        {
            MySqlCommand qry;

            if (query.id > 0)
            {
                // update
                qry = new MySqlCommand("update query set name=?name, sqltext=?sqltext, connection_id=?connection_id where id=?id", db);
                qry.Parameters.AddWithValue("?id", query.id);
            }
            else
            {
                // insert
                qry = new MySqlCommand("insert into query ( connection_id, name, sqltext) values (?connection_id,?name,?sqltext); select last_insert_id();", db);
            }

            qry.Parameters.AddWithValue("?connection_id", query.connection_id);
            qry.Parameters.AddWithValue("?name", query.name);
            qry.Parameters.AddWithValue("?sqltext", query.sqltext);

            qry.ExecuteNonQuery();

            if (query.id <= 0)
            {
                query.id = (int)qry.LastInsertedId;
            }

            return query.id;
        }

        public void DelQuery(AppDBQueryLink query)
        {
            MySqlCommand qry;

            if (query.id > 0)
            {
                qry = new MySqlCommand("delete from query where id=?id", db);
                qry.Parameters.AddWithValue("?id", query.id);

                qry.ExecuteNonQuery();

                query.id = 0;
            }
            else
            {
                // ???
            }
        }

        public void Dispose()
        {
            db.Dispose();
        }
    }
}
