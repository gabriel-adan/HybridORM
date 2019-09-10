namespace System.Data.ORM.Cfg
{
    public class MySQLConfiguration : ISQLConfiguration
    {
        public bool IsShowSql { get; private set; }
        public string ConnectionString { get; }

        public MySQLConfiguration(string connectionString)
        {
            ConnectionString = connectionString;
        }

        public string AssemblyTypeName()
        {
            return "MySql.Data";
        }

        public string ConnectionTypeName()
        {
            return "MySql.Data.MySqlClient.MySqlConnection";
        }

        public string CommandTypeName()
        {
            return "MySql.Data.MySqlClient.MySqlCommand";
        }

        public string LastInsertId()
        {
            return "SELECT LAST_INSERT_ID();";
        }

        public string SQLNot()
        {
            return "NOT";
        }

        public ISQLConfiguration ShowSql()
        {
            IsShowSql = true;
            return this;
        }
    }
}
