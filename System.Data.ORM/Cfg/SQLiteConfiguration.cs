namespace System.Data.ORM.Cfg
{
    public class SQLiteConfiguration : ISQLConfiguration
    {
        public bool IsShowSql { get; private set; }
        public string ConnectionString { get; }

        public SQLiteConfiguration(string connectionString)
        {
            ConnectionString = connectionString;
        }

        public string AssemblyTypeName()
        {
            return "System.Data.SQLite";
        }

        public string ConnectionTypeName()
        {
            return "System.Data.SQLite.SQLiteConnection";
        }

        public string CommandTypeName()
        {
            return "System.Data.SQLite.SQLiteCommand";
        }

        public string LastInsertId()
        {
            return "SELECT LAST_INSERT_ROWID();";
        }

        public string SQLNot()
        {
            return "IS NOT";
        }

        public ISQLConfiguration ShowSql()
        {
            IsShowSql = true;
            return this;
        }
    }
}
