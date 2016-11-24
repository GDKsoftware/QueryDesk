using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace QueryDesk
{
    public class AppDBDummyQuery
    {
        public long id { get; set; }

        public long connection_id { get; set; }

        public string name { get; set; }

        public string sqltext { get; set; }

        public AppDBDummyQuery(long id, long connection_id, string name, string sqltext)
        {
            this.id = id;
            this.connection_id = connection_id;
            this.name = name;
            this.sqltext = sqltext;
        }
    }

    public class AppDBDummyServer
    {
        public long id { get; set; }

        public string name { get; set; }

        public string host { get; set; }

        public long port { get; set; }

        public long type { get; set; }

        public string username { get; set; }

        public string password { get; set; }

        public string databasename { get; set; }

        public string extraparams { get; set; }

        public AppDBDummyServer()
        {
            this.id = 0;
            this.type = 0;
            this.name = string.Empty;
            this.host = string.Empty;
            this.port = 0;
            this.username = string.Empty;
            this.password = string.Empty;
            this.databasename = string.Empty;
            this.extraparams = string.Empty;
        }

        public AppDBDummyServer(long id, string name, long type, string host, long port, string username, string password, string database, string extraparams = "")
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

        public override string ToString()
        {
            return name;
        }
    }

    public class AppDBDummy : IAppDBServersAndQueries
    {
        private string connectionstring;

        private List<AppDBDummyServer> lstServers;
        private List<AppDBDummyQuery> lstQueries;

        public AppDBDummy(string connectionstring)
        {
            this.connectionstring = connectionstring;

            lstServers = new List<AppDBDummyServer>();
            lstServers.Add(new AppDBDummyServer(1, "Testserver 1", 1, "localhost", 3306, "root", "1234", "testdb"));

            lstQueries = new List<AppDBDummyQuery>();
            lstQueries.Add(new AppDBDummyQuery(1, 1, "Test Query 1", "select * from mytable where var1=:var1 and var2=:var2"));
        }

        public IEnumerable GetServerListing()
        {
            return lstServers;
        }

        public IEnumerable GetQueriesListing(long server_id)
        {
            var lst =
                from q in lstQueries
                where q.connection_id == server_id
                select q;

            return lst;
        }
    }
}
