using System.Collections.Generic;
using System.Data.ORM.CoreMap;

namespace System.Data.ORM.Mapping
{
    internal class EnumMap : IEnumMap
    {
        public Type Type { get; }
        public string TableName { get; private set; }
        public string PrimaryKeyName { get; private set; }
        public bool IsAutoincrement { get; private set; }
        public IDictionary<string, string> ColumnNames { get; }
        public IDictionary<string, IEntityMap> Entities { get; }
        public IDictionary<string, string> ForeignKeys { get; }
        public IDictionary<string, string> Keys { get; }
        public IDictionary<string, IEntityMap> Collections { get; }
        string IEntityMap.Select { get; set; }
        string IEntityMap.ForeignSelect { get; set; }
        public IDictionary<string, string> CollectionSelect { get; }

        public EnumMap(Type type)
        {
            Type = type;
            TableName = type.Name;
            PrimaryKeyName = "Id";
            IsAutoincrement = true;
        }
    }
}
