namespace System.Data.ORM.Cfg
{
    public interface ISQLConfiguration
    {
        bool IsShowSql { get; }

        string ConnectionString { get; }

        string AssemblyTypeName();

        string ConnectionTypeName();

        string CommandTypeName();

        string LastInsertId();

        string SQLNot();

        ISQLConfiguration ShowSql();
    }
}
