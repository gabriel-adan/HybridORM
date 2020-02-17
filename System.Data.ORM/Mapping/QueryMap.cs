using System.Data.ORM.CoreMap;
using System.Reflection;
using System.Linq.Expressions;
using System.Collections.Generic;

namespace System.Data.ORM.Mapping
{
    public class QueryMap<Q> : IQueryMapping<Q>, IPropertyMap, IQueryMap where Q : class
    {
        public Type Type { get; }
        public IDictionary<string, string> ColumnNames { get; }
        public string Query { get; }
        public string CurrentQuery { get; set; }
        private PropertyInfo CurrentProperty;
        private PropertyMapTranslator PropertyMapTranslator;

        public QueryMap(string query)
        {
            if (string.IsNullOrEmpty(query))
                throw new Exception("The query could not be null");
            Query = query;
            CurrentQuery = query;
            Type = typeof(Q);
            ColumnNames = new Dictionary<string, string>();
            PropertyMapTranslator = new PropertyMapTranslator();
            string nameSpace = string.Empty;
            foreach (var property in Type.GetProperties())
            {
                nameSpace = property.PropertyType.Namespace;
                if (nameSpace.Equals("System") && !nameSpace.Equals("System.Collections.Generic"))
                    ColumnNames.Add(property.Name, property.Name);
            }
        }

        public IPropertyMap Map<E>(Expression<Func<Q, E>> expression)
        {
            string propertyName = PropertyMapTranslator.Translate(expression);
            CurrentProperty = Type.GetProperty(propertyName);
            return this;
        }

        void IPropertyMap.ColumnName(string columnName)
        {
            string propertyName = CurrentProperty.Name;
            ColumnNames.Remove(propertyName);
            ColumnNames.Add(propertyName, columnName);
        }
    }
}