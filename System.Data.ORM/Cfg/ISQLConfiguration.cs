namespace System.Data.ORM.Cfg
{
    public interface ISQLConfiguration
    {
        string ConnectionString { get; }

        string AssemblyTypeName();

        string ConnectionTypeName();

        string CommandTypeName();

        string LastInsertId();

        string SQLNot();
    }
}
