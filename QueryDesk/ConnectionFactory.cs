namespace QueryDesk
{
    public static class ConnectionFactory
    {
        public static IQueryableConnection NewConnection(int type, string connectionstring)
        {
            if (type == 1)
            {
                return new MySQLQueryableConnection(connectionstring);
            }
            else if (type == 2)
            {
                return new MSSQLQueryableConnection(connectionstring);
            }
            else if (type == 3)
            {
                return new SQLiteQueryableConnection(connectionstring);
            }

            return null;
        }
    }
}
