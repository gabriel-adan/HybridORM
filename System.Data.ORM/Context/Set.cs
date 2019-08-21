using System.Reflection;
using System.Collections;
using System.Linq.Expressions;
using System.Collections.Generic;
using System.Data.ORM.CoreMap;
using System.Data.ORM.Queries;
using System.Data.ORM.Contracts;
using System.Data.ORM.Cfg;

namespace System.Data.ORM.Context
{
    internal class Set<T> : IDbSet<T> where T : class
    {
        IDbConnection connection;
        ISQLConfiguration configuration;
        Assembly assembly;
        IQuery<T> Query;
        IPersist<T> Persist;
        IModify<T> Modify;
        IDelete<T> Delete;
        Type CurrentType;
        string currentQuery;

        public Set(IDbConnection connection, ISQLConfiguration configuration, Assembly assembly, IQuery<T> query, IPersist<T> persist, IModify<T> modify, IDelete<T> delete)
        {
            this.connection = connection;
            this.configuration = configuration;
            this.assembly = assembly;
            Query = query;
            Persist = persist;
            Modify = modify;
            Delete = delete;
        }

        public T Find(object id)
        {
            try
            {
                Type type = typeof(T);
                CurrentType = type;
                T entity = null;
                string sql = Query.Find(id);
                using (IDbCommand command = CreateCommand(sql))
                {
                    using (IDataReader reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            entity = Activator.CreateInstance(type) as T;
                            foreach (var keyValueColumn in Query.EntityMap.ColumnNames)
                            {
                                var value = reader["_this." + keyValueColumn.Key];
                                PropertyInfo property = type.GetProperty(keyValueColumn.Key);
                                property.SetValue(entity, DataFormater.ParseToData(property, value));
                            }
                            foreach (var keyValuePair in Query.EntityMap.Entities)
                            {
                                object obj = Activator.CreateInstance(keyValuePair.Value.Type);
                                foreach (var keyValueColumn in keyValuePair.Value.ColumnNames)
                                {
                                    var value = reader[keyValuePair.Key + "." + keyValueColumn.Key];
                                    PropertyInfo property = keyValuePair.Value.Type.GetProperty(keyValueColumn.Key);
                                    property.SetValue(obj, DataFormater.ParseToData(property, value));
                                }
                                Query.EntityMap.Type.GetProperty(keyValuePair.Key).SetValue(entity, obj);
                            }
                        }
                        reader.Close();
                        reader.Dispose();
                        command.Dispose();
                    }
                }
                if (entity != null)
                {
                    FillEntities(entity);
                    FillCollections(entity);
                }
                return entity;
            }
            catch
            {
                throw;
            }
        }

        public T UniqueResult()
        {
            try
            {
                Type type = typeof(T);
                CurrentType = type;
                T entity = null;
                if (string.IsNullOrEmpty(currentQuery))
                    currentQuery = Query.EntityMap.Select;
                string sql = currentQuery + " LIMIT 1;";
                using (IDbCommand command = CreateCommand(sql))
                {
                    using (IDataReader reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            entity = Activator.CreateInstance(type) as T;
                            foreach (var keyValueColumn in Query.EntityMap.ColumnNames)
                            {
                                var value = reader["_this." + keyValueColumn.Key];
                                PropertyInfo property = type.GetProperty(keyValueColumn.Key);
                                property.SetValue(entity, DataFormater.ParseToData(property, value));
                            }
                            foreach (var keyValuePair in Query.EntityMap.Entities)
                            {
                                object obj = Activator.CreateInstance(keyValuePair.Value.Type);
                                foreach (var keyValueColumn in keyValuePair.Value.ColumnNames)
                                {
                                    var value = reader[keyValuePair.Key + "." + keyValueColumn.Key];
                                    PropertyInfo property = keyValuePair.Value.Type.GetProperty(keyValueColumn.Key);
                                    property.SetValue(obj, DataFormater.ParseToData(property, value));
                                }
                                Query.EntityMap.Type.GetProperty(keyValuePair.Key).SetValue(entity, obj);
                            }
                        }
                        reader.Close();
                        reader.Dispose();
                        command.Dispose();
                    }
                }
                if (entity != null)
                {
                    FillEntities(entity);
                    FillCollections(entity);
                }
                return entity;
            }
            catch
            {
                throw;
            }
        }

        public ICriteria<T> Where(Expression<Func<T, bool>> expression)
        {
            try
            {
                currentQuery = Query.EntityMap.Select + Query.Where(expression);
                return this;
            }
            catch
            {
                throw;
            }
        }

        public IOrder<T> GroupBy<E>(Expression<Func<T, E>> expression)
        {
            try
            {
                string groupBy = Query.GroupBy(expression);
                if (string.IsNullOrEmpty(currentQuery))
                    currentQuery = Query.EntityMap.Select + groupBy;
                else
                    currentQuery += groupBy;
                return this;
            }
            catch
            {
                throw;
            }
        }

        public IList<T> OrderBy<E>(Expression<Func<T, E>> expression)
        {
            try
            {
                Type type = Query.EntityMap.Type;
                CurrentType = type;
                IList<T> list = Activator.CreateInstance(typeof(List<>).MakeGenericType(type)) as IList<T>;
                string orderBy = Query.OrderBy(expression);
                if (string.IsNullOrEmpty(currentQuery))
                    currentQuery = Query.EntityMap.Select;
                string sql = currentQuery + " " + orderBy;
                currentQuery = null;
                list = Execute(sql);
                foreach (var obj in list)
                    FillEntities(obj);
                return list;
            }
            catch
            {
                throw;
            }
        }

        public IList<T> OrderBy<E>(Expression<Func<T, E>> expression, Order orderType)
        {
            try
            {
                Type type = Query.EntityMap.Type;
                CurrentType = type;
                IList<T> list = Activator.CreateInstance(typeof(List<>).MakeGenericType(type)) as IList<T>;
                string orderBy = Query.OrderBy(expression);
                if (string.IsNullOrEmpty(currentQuery))
                    currentQuery = Query.EntityMap.Select;
                string sql = currentQuery + " " + orderBy + " " + orderType + ";";
                currentQuery = null;
                list = Execute(sql);
                foreach (var obj in list)
                    FillEntities(obj);
                return list;
            }
            catch
            {
                throw;
            }
        }

        public IList<T> ToList()
        {
            try
            {
                Type type = Query.EntityMap.Type;
                CurrentType = type;
                IList<T> list = Activator.CreateInstance(typeof(List<>).MakeGenericType(type)) as IList<T>;
                if (string.IsNullOrEmpty(currentQuery))
                    currentQuery = Query.ToList();
                string sql = currentQuery;
                currentQuery = null;
                list = Execute(sql);
                foreach (var obj in list)
                    FillEntities(obj);
                return list;
            }
            catch
            {
                throw;
            }
        }

        public bool Save(T entity)
        {
            try
            {
                string sql = Persist.Insert(entity);
                using (IDbCommand command = CreateCommand(sql))
                {
                    if (command.ExecuteNonQuery() > 0)
                    {
                        if (!string.IsNullOrEmpty(Query.EntityMap.PrimaryKeyName))
                        {
                            if (Query.EntityMap.IsAutoincrement)
                            {
                                command.CommandText = "SELECT LAST_INSERT_ID() AS Id;";
                                string res = command.ExecuteScalar() + "";
                                int id = int.Parse(res);
                                PropertyInfo property = Query.EntityMap.Type.GetProperty(Query.EntityMap.PrimaryKeyName);
                                property.SetValue(entity, DataFormater.ParseToData(property, id));
                            }
                        }
                        command.Dispose();
                        return true;
                    }
                    else
                    {
                        command.Dispose();
                        return false;
                    }
                }
            }
            catch
            {
                throw;
            }
        }

        public bool Update(T entity)
        {
            try
            {
                string sql = Modify.Update(entity);
                using (IDbCommand command = CreateCommand(sql))
                {
                    bool result = command.ExecuteNonQuery() > 0;
                    command.Dispose();
                    return result;
                }
            }
            catch
            {
                throw;
            }
        }

        public bool Remove(T entity)
        {
            try
            {
                string sql = Delete.Remove(entity);
                using (IDbCommand command = CreateCommand(sql))
                {
                    bool result = command.ExecuteNonQuery() > 0;
                    command.Dispose();
                    return result;
                }
            }
            catch
            {
                throw;
            }
        }

        void FillEntities(object entity)
        {
            Type type = entity.GetType();
            IEntityMap entityMap;
            Cfg.Configuration.Mappings.TryGetValue(type.Name, out entityMap);
            foreach (var keyValuePair in entityMap.Entities)
            {
                if (CurrentType != keyValuePair.Value.Type && !keyValuePair.Value.Type.IsEnum)
                {
                    IEntityMap foreignEntity = keyValuePair.Value;
                    object obj = type.GetProperty(keyValuePair.Key).GetValue(entity);
                    if (obj != null)
                    {
                        if (!string.IsNullOrEmpty(foreignEntity.PrimaryKeyName))
                        {
                            object id = foreignEntity.Type.GetProperty(foreignEntity.PrimaryKeyName).GetValue(obj);
                            string sql = foreignEntity.ForeignSelect + "WHERE _this." + foreignEntity.PrimaryKeyName + " = " + DataFormater.ParseToSQL(id) + ";";
                            foreach (var keyValue in foreignEntity.Entities)
                            {
                                IEntityMap map = keyValue.Value;
                                using (IDbCommand command = CreateCommand(sql))
                                {
                                    using (IDataReader reader = command.ExecuteReader())
                                    {
                                        if (reader.Read())
                                        {
                                            object o = Activator.CreateInstance(map.Type);
                                            foreach (var kv in map.ColumnNames)
                                            {
                                                string propertyName = kv.Key;
                                                var value = reader[keyValue.Key + "." + propertyName];
                                                propertyName = propertyName.Replace(keyValue.Key + ".", "");
                                                PropertyInfo property = map.Type.GetProperty(propertyName);
                                                property.SetValue(o, DataFormater.ParseToData(property, value));
                                            }

                                            foreignEntity.Type.GetProperty(keyValue.Key).SetValue(obj, o);
                                            reader.Close();
                                            reader.Dispose();
                                            command.Dispose();
                                            FillEntities(o);
                                        }
                                    }
                                }
                            }
                        }
                        else
                        {

                        }
                    }
                }
                else
                {
                    // la entidad es del mismo tipo que T
                }
            }
        }

        void FillCollections(object entity)
        {
            try
            {
                Type type = entity.GetType();
                IEntityMap entityMap;
                Cfg.Configuration.Mappings.TryGetValue(type.Name, out entityMap);
                if (string.IsNullOrEmpty(entityMap.PrimaryKeyName))
                {

                }
                else
                {
                    object id = type.GetProperty(entityMap.PrimaryKeyName).GetValue(entity);
                    foreach (var keyValuePair in entityMap.Collections)
                    {
                        IEntityMap itemMap = keyValuePair.Value;
                        string sql;
                        entityMap.CollectionSelect.TryGetValue(keyValuePair.Key, out sql);
                        IList collection = Activator.CreateInstance(typeof(List<>).MakeGenericType(itemMap.Type)) as IList;
                        type.GetProperty(keyValuePair.Key).SetValue(entity, collection);
                        foreach (var kvf in itemMap.ForeignKeys)
                        {
                            if (sql.Contains("{" + kvf.Key + "}"))
                            {
                                sql = sql.Replace("{" + kvf.Key + "}", DataFormater.ParseToSQL(id) + "");
                                using (IDbCommand command = CreateCommand(sql))
                                {
                                    using (IDataReader reader = command.ExecuteReader())
                                    {
                                        while (reader.Read())
                                        {
                                            object item = Activator.CreateInstance(itemMap.Type);
                                            foreach (var keyValueColumn in itemMap.ColumnNames)
                                            {
                                                var value = reader[keyValuePair.Key + "." + keyValueColumn.Key];
                                                PropertyInfo property = itemMap.Type.GetProperty(keyValueColumn.Key);
                                                property.SetValue(item, DataFormater.ParseToData(property, value));
                                            }
                                            foreach (var keyValue in itemMap.Entities)
                                            {
                                                IEntityMap compositeEntity = keyValue.Value;
                                                if (compositeEntity.Type != CurrentType)
                                                {
                                                    object obj = Activator.CreateInstance(compositeEntity.Type);
                                                    foreach (var keyValueColumn in compositeEntity.ColumnNames)
                                                    {
                                                        var value = reader[keyValue.Key + "." + keyValueColumn.Key];
                                                        PropertyInfo property = compositeEntity.Type.GetProperty(keyValueColumn.Key);
                                                        property.SetValue(obj, DataFormater.ParseToData(property, value));
                                                    }
                                                    itemMap.Type.GetProperty(keyValue.Key).SetValue(item, obj);
                                                }
                                                else
                                                {
                                                    itemMap.Type.GetProperty(kvf.Key).SetValue(item, entity);
                                                }
                                            }
                                            collection.Add(item);
                                        }
                                        reader.Close();
                                        reader.Dispose();
                                        command.Dispose();
                                    }
                                }
                            }
                        }
                    }
                }
            }
            catch
            {
                throw;
            }
        }

        IList<T> Execute(string sql)
        {
            try
            {
                Type type = Query.EntityMap.Type;
                CurrentType = type;
                IList<T> list = Activator.CreateInstance(typeof(List<>).MakeGenericType(type)) as IList<T>;
                using (IDbCommand command = CreateCommand(sql))
                {
                    using (IDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            T entity = Activator.CreateInstance(type) as T;
                            foreach (var keyValueColumn in Query.EntityMap.ColumnNames)
                            {
                                var value = reader["_this." + keyValueColumn.Key];
                                PropertyInfo property = type.GetProperty(keyValueColumn.Key);
                                property.SetValue(entity, DataFormater.ParseToData(property, value));
                            }
                            foreach (var keyValuePair in Query.EntityMap.Entities)
                            {
                                if (keyValuePair.Value.Type.IsEnum)
                                {
                                    var value = reader[keyValuePair.Key + "." + keyValuePair.Value.PrimaryKeyName];

                                    if (Enum.IsDefined(keyValuePair.Value.Type, value))
                                    {
                                        Enum @enum = (Enum)Enum.Parse(keyValuePair.Value.Type, value + "");
                                        Query.EntityMap.Type.GetProperty(keyValuePair.Key).SetValue(entity, @enum);
                                    }
                                    else
                                    {
                                        throw new Exception("El tipo enum [" + keyValuePair.Value.Type + "] no tiene definido un valor para asociado para el valor [" + value + "].");
                                    }
                                }
                                else
                                {
                                    object obj = Activator.CreateInstance(keyValuePair.Value.Type);
                                    foreach (var keyValueColumn in keyValuePair.Value.ColumnNames)
                                    {
                                        var value = reader[keyValuePair.Key + "." + keyValueColumn.Key];
                                        PropertyInfo property = keyValuePair.Value.Type.GetProperty(keyValueColumn.Key);
                                        property.SetValue(obj, DataFormater.ParseToData(property, value));
                                    }
                                    Query.EntityMap.Type.GetProperty(keyValuePair.Key).SetValue(entity, obj);
                                }
                            }
                            list.Add(entity);
                        }
                        reader.Close();
                        reader.Dispose();
                        command.Dispose();
                    }
                }
                foreach (var obj in list)
                    FillEntities(obj);
                return list;
            }
            catch
            {
                throw;
            }
        }

        IDbCommand CreateCommand(string query)
        {
            IDbCommand command = assembly.CreateInstance(configuration.CommandTypeName(), true) as IDbCommand;
            command.Connection = connection as IDbConnection;
            command.CommandText = query;
            return command;
        }
    }
}
