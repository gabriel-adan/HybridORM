namespace System.Data.ORM.Mapping
{
    public interface IPropertyIdMap
    {
        void ColumnName(string columnName);

        IPropertyIdMap IsNotAutoIncrement();
    }
}
