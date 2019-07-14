using System.Collections.Generic;

namespace System.Data.ORM.CoreMap
{
    public interface IEntityMap
    {
        Type Type { get; }
        string TableName { get; }
        string PrimaryKeyName { get; }
        bool IsAutoincrement { get; }
        IDictionary<string, string> ColumnNames { get; }
        IDictionary<string, IEntityMap> Entities { get; }
        IDictionary<string, string> ForeignKeys { get; }
        IDictionary<string, string> Keys { get; }
        IDictionary<string, IEntityMap> Collections { get; }

        string Select { get; set; }
        string ForeignSelect { get; set; }
        IDictionary<string, string> CollectionSelect { get; }
    }
}
