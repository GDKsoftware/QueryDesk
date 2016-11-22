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
        public static Dictionary<int, string> List()
        {
            var r = new Dictionary<int, string>();
            r.Add((int)AppDBServerType.Void, string.Empty);
            r.Add((int)AppDBServerType.MySQL, "MySQL");
            r.Add((int)AppDBServerType.MSSQL, "MSSQL");
            r.Add((int)AppDBServerType.SQLite, "SQLite");

            return r;
        }
    }
}
