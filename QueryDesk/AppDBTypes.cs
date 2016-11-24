using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QueryDesk
{
    public enum AppDBServerType
    {
        Void = 0, MySQL = 1, MSSQL = 2, SQLite = 3
    }

    public static class AppDBTypes
    {
        public static Dictionary<long, string> List()
        {
            var r = new Dictionary<long, string>();
            r.Add((long)AppDBServerType.Void, string.Empty);
            r.Add((long)AppDBServerType.MySQL, "MySQL");
            r.Add((long)AppDBServerType.MSSQL, "MSSQL");
            r.Add((long)AppDBServerType.SQLite, "SQLite");

            return r;
        }
    }
}
