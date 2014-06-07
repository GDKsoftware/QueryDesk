using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QueryDesk
{
    public class QueryComposerHelper
    {
        protected MySQLQueryableConnection DBConnection = null;
        protected Dictionary<string, List<string>> DBLayout = null;

        public QueryComposerHelper(MySQLQueryableConnection connection)
        {
            DBConnection = connection;

            InitializeLayout();
        }

        protected void InitializeLayout()
        {
            DBLayout = new Dictionary<string, List<string>>();

            foreach (var tablename in DBConnection.ListTableNames())
            {
                var fields = DBConnection.ListFieldNames(tablename);
                DBLayout.Add(tablename, fields);
            }
        }

        //public List<string> SenseAt(int row, int col, )
    }
}
