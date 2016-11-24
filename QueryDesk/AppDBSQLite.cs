using System;
using System.Collections;
using System.Data;
using System.Data.SQLite;

namespace QueryDesk
{
    public class AppDBSQLite : IAppDBServersAndQueries, IAppDBEditableServers, IAppDBEditableQueries, IDisposable
    {
        private string connectionstring;
        private SQLiteConnection db;

        public AppDBSQLite(string connectionstring)
        {
            this.connectionstring = connectionstring;

            try
            {
                db = new SQLiteConnection(connectionstring);

                db.Open(); // throws exception if failed to connect

                CreateTables();
            }
            catch (SQLiteException)
            {
                // todo: make unique exception that everyone AppDB class can use
                throw;
            }
        }

        private void ExecuteDDL(string sql)
        {
            var cmd = new SQLiteCommand(sql, db);
            cmd.ExecuteNonQuery();
        }

        private void CreateTables()
        {
            var queryable = new SQLiteQueryableConnection(this.connectionstring);
            queryable.Connect();

            var tables = queryable.ListTableNames();

            if (!tables.Contains("connection"))
            {
                var ddl =
                    "CREATE TABLE `connection` (" +
                    " `id` INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT," +
                    " `type` INTEGER NOT NULL DEFAULT 1," +
                    " `name` TEXT NOT NULL," +
                    " `host` TEXT NOT NULL," +
                    " `port` INTEGER NOT NULL," +
                    " `username` TEXT NOT NULL," +
                    " `password` TEXT NOT NULL," +
                    " `databasename` TEXT NOT NULL," +
                    " `extraparams` TEXT NOT NULL" +
                    "); ";

                ExecuteDDL(ddl);
            }

            if (!tables.Contains("query"))
            {
                var ddl =
                    "CREATE TABLE `query` (" +
                    " `id` INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT," +
                    " `connection_id` INTEGER NOT NULL," +
                    " `name` TEXT NOT NULL," +
                    " `sqltext` TEXT NOT NULL" +
                    " ); ";

                ExecuteDDL(ddl);
            }
        }

        public IEnumerable GetServerListing()
        {
            var adapter = new SQLiteDataAdapter();
            var cmd = new SQLiteCommand("select id, type, name, host, port, username, password, databasename, extraparams from connection order by name asc", db);

            adapter.SelectCommand = cmd;

            DataSet ds = new DataSet();
            adapter.Fill(ds, "connection");

            var dt = ds.Tables["connection"];

            return dt.DefaultView;
        }

        public IEnumerable GetQueriesListing(long server_id)
        {
            var adapter = new SQLiteDataAdapter();
            var cmd = new SQLiteCommand("select id, connection_id, name, sqltext from query where connection_id=@connection_id order by name asc", db);
            cmd.Parameters.AddWithValue("@connection_id", server_id);

            adapter.SelectCommand = cmd;

            DataSet ds = new DataSet();
            adapter.Fill(ds, "query");

            var dt = ds.Tables["query"];
            return dt.DefaultView;
        }

        public long SaveServer(AppDBServerLink server)
        {
            SQLiteCommand qry;

            if (server.id > 0)
            {
                // update
                qry = new SQLiteCommand("update connection set type=@type, name=@name, host=@host, port=@port, username=@username, password=@password, databasename=@databasename, extraparams=@extraparams where id=@id", db);
                qry.Parameters.AddWithValue("@id", server.id);
            }
            else
            {
                // insert
                qry = new SQLiteCommand("insert into connection ( type, name, host, port, username, password, databasename, extraparams) values (@type,@name,@host,@port,@username,@password,@databasename,@extraparams);", db);
            }

            qry.Parameters.AddWithValue("@type", server.type);
            qry.Parameters.AddWithValue("@name", server.name);
            qry.Parameters.AddWithValue("@host", server.host);
            qry.Parameters.AddWithValue("@port", server.port);
            qry.Parameters.AddWithValue("@username", server.username);
            qry.Parameters.AddWithValue("@password", server.password);
            qry.Parameters.AddWithValue("@databasename", server.databasename);
            qry.Parameters.AddWithValue("@extraparams", server.extraparams);

            qry.ExecuteNonQuery();

            // get inserted id and assign to in server.id
            if (server.id <= 0)
            {
                qry.CommandText = "select last_insert_rowid()";
                server.id = (long)qry.ExecuteScalar();
            }

            return server.id;
        }

        public void DelServer(AppDBServerLink server)
        {
            SQLiteCommand qry;

            if (server.id > 0)
            {
                qry = new SQLiteCommand("delete from connection where id=@id", db);
                qry.Parameters.AddWithValue("@id", server.id);

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
            SQLiteCommand qry;

            if (query.id > 0)
            {
                // update
                qry = new SQLiteCommand("update query set name=@name, sqltext=@sqltext, connection_id=@connection_id where id=@id", db);
                qry.Parameters.AddWithValue("@id", query.id);
            }
            else
            {
                // insert
                qry = new SQLiteCommand("insert into query ( connection_id, name, sqltext) values (@connection_id,@name,@sqltext);", db);
            }

            qry.Parameters.AddWithValue("@connection_id", query.connection_id);
            qry.Parameters.AddWithValue("@name", query.name);
            qry.Parameters.AddWithValue("@sqltext", query.sqltext);

            qry.ExecuteNonQuery();

            if (query.id <= 0)
            {
                qry.CommandText = "select last_insert_rowid()";
                query.id = (long)qry.ExecuteScalar();
            }

            return query.id;
        }

        public void DelQuery(AppDBQueryLink query)
        {
            SQLiteCommand qry;

            if (query.id > 0)
            {
                qry = new SQLiteCommand("delete from query where id=@id", db);
                qry.Parameters.AddWithValue("@id", query.id);

                qry.ExecuteNonQuery();

                query.id = 0;
            }
            else
            {
                // @@@
            }
        }

        public void Dispose()
        {
            db.Dispose();
        }
    }
}
