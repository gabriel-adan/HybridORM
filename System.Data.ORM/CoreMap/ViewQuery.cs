using System.Data.ORM.Contracts;
using System.Linq.Expressions;

namespace System.Data.ORM.CoreMap
{
    internal class ViewQuery<V> : IViewQuery<V> where V : class
    {
        public IViewMap ViewMap { get; }
        readonly ViewQueryTranslator<V> ViewQueryTranslator;

        public ViewQuery(IViewMap viewMap)
        {
            ViewMap = viewMap;
            ViewQueryTranslator = new ViewQueryTranslator<V>(ViewMap);
            string columnNames = string.Empty;
            foreach(var keyValue in ViewMap.ColumnNames)
            {
                columnNames += keyValue.Value + " AS " + keyValue.Key + ", ";
            }
            columnNames += "-";
            columnNames = columnNames.Replace(", -", "");
            ViewMap.Query = "SELECT " + columnNames + " FROM " + viewMap.Name;
        }

        public string Where<E>(Expression<Func<V, E>> expression)
        {
            return " WHERE " + ViewQueryTranslator.Where(expression);
        }

        public string GroupBy<E>(Expression<Func<V, E>> expression)
        {
            return " GROUP BY " + ViewQueryTranslator.Where(expression);
        }

        public string OrderBy<E>(Expression<Func<V, E>> expression)
        {
            return " ORDER BY " + ViewQueryTranslator.Where(expression);
        }

        public string ToList()
        {
            return ViewMap.Query;
        }
    }
}
