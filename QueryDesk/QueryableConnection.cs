using System.Collections.Generic;
using System.Data;

namespace QueryDesk
{
    public interface IQueryableConnection
    {
        bool Connect();

        void Disconnect();

        bool Query(StoredQuery qry);

        DataTable ResultsAsDataTable();

        void CloseQuery();

        List<string> ListTableNames();

        Dictionary<string, string> ListFieldNames(string tablename);

        char GetParamPrefixChar();
    }
}
