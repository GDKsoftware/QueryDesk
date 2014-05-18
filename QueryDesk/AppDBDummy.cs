using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QueryDesk
{
    class AppDBDummyQuery
    {
        public int id {get; set;}
        public int connection_id {get; set;}
        public string name {get; set;}
        public string sqltext {get; set;}

        public AppDBDummyQuery(int id, int connection_id, string name, string sqltext)
        {
            this.id = id;
            this.connection_id = connection_id;
            this.name = name;
            this.sqltext = sqltext;
        }
    }

    class AppDBDummyServer
    {
        public int id { get; set; }
        public string name { get; set; }
        public string host { get; set; }
        public int port { get; set; }
        public AppDBServerType type { get; set; }
        public string username { get; set; }
        public string password { get; set; }
        public string databasename { get; set; }
        public string extraparams { get; set; }

        public AppDBDummyServer()
        {
            this.id = 0;
            this.name = "";
            this.host = "";
            this.port = 0;
            this.type = AppDBServerType.Void;
            this.username = "";
            this.password = "";
            this.databasename = "";
            this.extraparams = "";
        }

        public AppDBDummyServer(int id, string name, AppDBServerType type, string host, int port, string username, string password, string database, string extraparams = "")
        {
            this.id = id;
            this.name = name;
            this.host = host;
            this.port = port;
            this.type = type;
            this.username = username;
            this.password = password;
            this.databasename = database;
            this.extraparams = extraparams;
        }

        public override string ToString() {
            return name;
        }
    }

    class AppDBDummy: IAppDBServersAndQueries
    {
        private string connectionstring;

        private List<AppDBDummyServer> lstServers;
        private List<AppDBDummyQuery> lstQueries;

        public AppDBDummy(string connectionstring)
        {
            this.connectionstring = connectionstring;

            lstServers = new List<AppDBDummyServer>();
            lstServers.Add(new AppDBDummyServer(1, "Testserver 1", AppDBServerType.Void, "localhost", 3306, "root", "1234", "testdb"));

            lstQueries = new List<AppDBDummyQuery>();
            lstQueries.Add(new AppDBDummyQuery(1,1,"Test Query 1", "select * from mytable where var1=:var1 and var2=:var2"));
        }

        public IEnumerable getServerListing()
        {
            return lstServers;
        }

        public IEnumerable getQueriesListing(long server_id)
        {
            var lst =
                from q in lstQueries
                where q.connection_id == server_id
                select q;

            return lst;
        }
    }
}
