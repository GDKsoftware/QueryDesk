using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QueryDesk
{
    class StoredQuery
    {
        protected string sqltext;
        protected List<string> parameters;

        public string SQL
        {
            get { return sqltext; }
            set { sqltext = value; ParseParams(); }
        }

        protected void ParseParams()
        {
            parameters.Clear();

            var inparam = false;
            var paramname = "";

            var i = 0;
            var c = sqltext.Length;
            while (i < c)
            {
                if (sqltext[i] == ':')
                {
                    inparam = true;
                }
                else if (inparam && (char.IsLetterOrDigit(sqltext[i]) || sqltext[i] == '_'))
                {
                    paramname += sqltext[i];
                }
                else
                {
                    inparam = false;
                    parameters.Add(paramname);
                }
            }
        }
    }

    class StoredQueries
    {
        public Dictionary<string, StoredQuery> Queries;

    }
}
