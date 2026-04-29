using System;
using Oracle.ManagedDataAccess.Client;

namespace MyDiary.Data
{
    internal class DbHelper
    {
        private static string connStr =
       "User Id=scott;Password=tiger;Data Source=localhost:1521/orcl;";

        public static OracleConnection GetConnection()
        {
            return new OracleConnection(connStr);
        }
    }
}
