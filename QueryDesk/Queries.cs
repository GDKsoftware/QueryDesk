using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QueryDesk
{
    public class StoredQuery
    {
        protected string sqltext;
        public Dictionary<string, object> parameters = new Dictionary<string, object>();

        public string SQL
        {
            get { return sqltext; }
            set { sqltext = value; ParseParams(); }
        }

        public Boolean HasParameters()
        {
            return parameters.Count > 0;
        }

        /// <summary>
        /// Inserts string s into list sorted by descending stringlength
        /// </summary>
        /// <param name="lst">list to put string s into</param>
        /// <param name="s">string</param>
        public static void AddLenSorted(List<string> lst, string s)
        {
            for (int i = 0; i < lst.Count; i++)
            {
                var p = lst[i];

                if (s.Length > p.Length)
                {
                    lst.Insert(i, s);
                    return;
                }
            }

            lst.Add(s);
        }

        protected void ParseParams()
        {
            parameters.Clear();

            var inparam = false;
            var paramname = "";

            List<string> lstParams = new List<string>();

            var i = 0;
            var c = sqltext.Length;
            while (i < c)
            {
                // start of parameter with : or ?
                if ((sqltext[i] == ':') || (sqltext[i] == '?') || (sqltext[i] == '@'))
                {
                    inparam = true;
                }
                else if (inparam && (char.IsLetterOrDigit(sqltext[i]) || sqltext[i] == '_'))
                {
                    // parameter name continues as long as theres an alphanum char or underscore
                    paramname += sqltext[i];
                }
                else if (inparam)
                {
                    // some other char, probably ending the parameter name
                    AddLenSorted(lstParams, paramname);
                    inparam = false;
                    paramname = "";
                }

                i++;
            }

            // if we're on the end of string and we haven't stored the last paramname yet
            if (inparam && (paramname != ""))
            {
                AddLenSorted(lstParams, paramname);
            }

            // add parameters sorted to dictionary
            foreach (var p in lstParams)
            {
                parameters.Add(p, null);
            }
        }

        /// <summary>
        /// Generalize parameters prefixes to given character (mysql '?', mssql '@')
        /// </summary>
        /// <param name="rewriteparamprefixchar">char</param>
        public void RewriteParameters(char rewriteparamprefixchar)
        {
            foreach (var paramname in parameters.Keys)
            {
                sqltext = sqltext.Replace(':' + paramname, rewriteparamprefixchar + paramname);
                sqltext = sqltext.Replace('?' + paramname, rewriteparamprefixchar + paramname);
                sqltext = sqltext.Replace('@' + paramname, rewriteparamprefixchar + paramname);
            }
        }

        public StoredQuery(string sql)
        {
            this.SQL = sql;
        }

        public StoredQuery(StoredQuery qry)
        {
            this.SQL = qry.SQL;
            CopyParamsFrom(qry);
        }

        public void CopyParamsFrom(StoredQuery qry)
        {
            foreach (var p in qry.parameters)
            {
                this.parameters[p.Key] = p.Value;
            }
        }

        public override string ToString()
        {
            return this.SQL;
        }
    }
}
