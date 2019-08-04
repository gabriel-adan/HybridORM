using System.Reflection;
using System.Linq.Expressions;
using System.Data.ORM.CoreMap;
using System.Collections.Generic;

namespace System.Data.ORM.Mapping
{
    public class ViewMap<V> : IViewMapping<V>, IPropertyMap, IViewMap where V : class
    {
        public Type Type { get; }
        public IDictionary<string, string> ColumnNames { get; }
        public string Name { get; }
        string IViewMap.Query { get; set; }
        private PropertyInfo CurrentProperty;
        private PropertyMapTranslator PropertyMapTranslator;

        public ViewMap(string viewName)
        {
            Type = typeof(V);
            Name = string.IsNullOrEmpty(viewName) ? Type.Name : viewName;
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

        public IPropertyMap Map<E>(Expression<Func<V, E>> expression)
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
