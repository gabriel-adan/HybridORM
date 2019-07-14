using System.Reflection;
using System.Linq.Expressions;
using System.Collections.Generic;
using System.Data.ORM.CoreMap;

namespace System.Data.ORM.Mapping
{
    public abstract class ClassMap<T> : IEntityMapping<T>, IPropertyIdMap, IPropertyMap, IPropertyKeyMap, IForeignKeyMap<T>, IToManyMap<T>, IEntityMap where T : class
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
        private PropertyInfo CurrentProperty;
        private Type CurrentItemType;
        private string CurrentForeignPropertyName;
        private PropertyMapTranslator PropertyMapTranslator;
        private PropertyForeignKeyTranslator PropertyForeignKeyTranslator;
        private PropertyKeyMapTranslator PropertyKeyMapTranslator;

        public ClassMap()
        {
            Type = typeof(T);
            TableName = Type.Name;
            PrimaryKeyName = Type.GetProperty("Id") == null ? null : "Id";
            IsAutoincrement = !string.IsNullOrEmpty(PrimaryKeyName);
            ColumnNames = new Dictionary<string, string>();
            Entities = new Dictionary<string, IEntityMap>();
            ForeignKeys = new Dictionary<string, string>();
            Keys = new Dictionary<string, string>();
            Collections = new Dictionary<string, IEntityMap>();
            CollectionSelect = new Dictionary<string, string>();
            PropertyMapTranslator = new PropertyMapTranslator();
            PropertyForeignKeyTranslator = new PropertyForeignKeyTranslator();
            PropertyKeyMapTranslator = new PropertyKeyMapTranslator();

            string nameSpace = string.Empty;
            foreach (var property in Type.GetProperties())
            {
                nameSpace = property.PropertyType.Namespace;
                if (nameSpace.Equals("System") && !nameSpace.Equals("System.Collections.Generic"))
                    ColumnNames.Add(property.Name, property.Name);
                if (!nameSpace.Equals("System") && !nameSpace.Equals("System.Collections.Generic"))
                    ForeignKeys.Add(property.Name, property.Name + "_Id");
            }
        }

        public IPropertyIdMap Id<TId>(Expression<Func<T, TId>> expression)
        {
            string propertyName = PropertyMapTranslator.Translate(expression);
            PrimaryKeyName = propertyName;
            CurrentProperty = Type.GetProperty(propertyName);
            return this;
        }

        public IPropertyMap Map<TId>(Expression<Func<T, TId>> expression)
        {
            string propertyName = PropertyMapTranslator.Translate(expression);
            CurrentProperty = Type.GetProperty(propertyName);
            return this;
        }

        public void Table(string tableName)
        {
            TableName = tableName;
        }

        void IPropertyIdMap.ColumnName(string columnName)
        {
            PrimaryKeyName = columnName;
            string propertyName = CurrentProperty.Name;
            ColumnNames.Remove(propertyName);
            ColumnNames.Add(propertyName, columnName);
        }

        IPropertyIdMap IPropertyIdMap.IsNotAutoIncrement()
        {
            IsAutoincrement = false;
            return this;
        }

        void IPropertyMap.ColumnName(string columnName)
        {
            string propertyName = CurrentProperty.Name;
            ColumnNames.Remove(propertyName);
            ColumnNames.Add(propertyName, columnName);
        }

        public IForeignKeyMap<T> ForeignKey<E>(Expression<Func<T, E>> expression)
        {
            CurrentForeignPropertyName = PropertyForeignKeyTranslator.Translate(expression);
            CurrentProperty = Type.GetProperty(CurrentForeignPropertyName);
            return this;
        }

        void IForeignKeyMap<T>.ColumnName(string columnName)
        {
            if (CurrentProperty != null)
            {
                if (CurrentProperty.PropertyType.Namespace.Equals("System"))
                {
                    Keys.Remove(CurrentProperty.Name);
                    Keys.Add(CurrentProperty.Name, columnName);
                }
                else
                {
                    IEntityMap entity = Activator.CreateInstance(typeof(EntityMap<>).MakeGenericType(CurrentProperty.PropertyType)) as IEntityMap;
                    string propertyName = CurrentProperty.Name;
                    if (string.IsNullOrEmpty(entity.PrimaryKeyName) && entity.ForeignKeys.Count > 0)
                    {
                        ForeignKeys.Remove(propertyName);
                        if (string.IsNullOrEmpty(CurrentForeignPropertyName))
                        {
                            ForeignKeys.Add(columnName, columnName);
                        }
                        else
                        {
                            ForeignKeys.Remove(propertyName);
                            ForeignKeys.Add(propertyName, columnName);
                            CurrentForeignPropertyName = null;
                            CurrentProperty = null;
                        }
                    }
                    else
                    {
                        ForeignKeys.Remove(propertyName);
                        ForeignKeys.Add(propertyName, columnName);
                    }
                }
            }
            else
            {
                if (!string.IsNullOrEmpty(CurrentForeignPropertyName))
                {
                    if (CurrentForeignPropertyName.Contains("."))
                    {
                        string[] entities = CurrentForeignPropertyName.Split('.');
                        PropertyInfo property = Type.GetProperty(entities[0]);
                        if (property != null)
                        {
                            IEntityMap entity = Activator.CreateInstance(typeof(EntityMap<>).MakeGenericType(property.PropertyType)) as IEntityMap;
                            if (string.IsNullOrEmpty(entity.PrimaryKeyName))
                            {
                                ForeignKeys.Remove(property.Name);
                            }
                        }
                    }
                    ForeignKeys.Add(CurrentForeignPropertyName, columnName);
                }
            }
        }

        void IToManyMap<T>.ColumnName(string columnName)
        {
            IEntityMap entityMap;
            Cfg.Configuration.Mappings.TryGetValue(CurrentItemType.Name, out entityMap);
            entityMap.ForeignKeys.Remove(CurrentForeignPropertyName);
            entityMap.ForeignKeys.Add(CurrentForeignPropertyName, columnName);
            CurrentItemType = null;
            CurrentForeignPropertyName = null;
        }

        public IToManyMap<T> HasMany<E>(Expression<Func<T, IList<E>>> expression)
        {
            CurrentItemType = typeof(E);
            IEntityMap entityMap;
            Cfg.Configuration.Mappings.TryGetValue(CurrentItemType.Name, out entityMap);
            foreach (var property in CurrentItemType.GetProperties())
            {
                if (!property.PropertyType.Namespace.Equals("System") && !property.PropertyType.Namespace.Equals("System.Collections.Generic"))
                {
                    if (Type == property.PropertyType)
                    {
                        string columnName;
                        entityMap.ForeignKeys.TryGetValue(property.Name, out columnName);
                        if (!string.IsNullOrEmpty(columnName))
                        {
                            CurrentForeignPropertyName = property.Name;
                            break;
                        }
                    }
                }
            }
            return this;
        }

        void IPropertyKeyMap.ColumnName(string columnName)
        {
            Keys.Remove(CurrentForeignPropertyName);
            Keys.Add(CurrentForeignPropertyName, columnName);
        }

        public IPropertyKeyMap Key<EKey>(Expression<Func<T, EKey>> expression)
        {
            Type type = typeof(T);
            CurrentForeignPropertyName = PropertyKeyMapTranslator.Translate(expression);
            Keys.Add(CurrentForeignPropertyName, CurrentForeignPropertyName);
            CurrentProperty = type.GetProperty(CurrentForeignPropertyName);
            return this;
        }
    }
}
