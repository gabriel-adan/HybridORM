using System.Collections.Generic;

namespace System.Data.ORM.CoreMap
{
    internal interface IQueryMap
    {
        Type Type { get; }
        IDictionary<string, string> ColumnNames { get; }
        string Query { get; }
        string CurrentQuery { get; set; }
    }
}