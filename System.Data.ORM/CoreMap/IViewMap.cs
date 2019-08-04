using System.Collections.Generic;

namespace System.Data.ORM.CoreMap
{
    internal interface IViewMap
    {
        Type Type { get; }
        IDictionary<string, string> ColumnNames { get; }
        string Name { get; }
        string Query { get; set; }
    }
}
