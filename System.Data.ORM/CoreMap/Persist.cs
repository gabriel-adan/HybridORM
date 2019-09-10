using System.Data.ORM.Contracts;

namespace System.Data.ORM.CoreMap
{
    internal class Persist<T> : IPersist<T> where T : class
    {
        string Query;
        IEntityMap EntityMap;

        public Persist(IEntityMap entityMap)
        {
            EntityMap = entityMap;
            string columnNames = string.Empty, values = string.Empty;
            foreach (var keyValueColumn in EntityMap.ColumnNames)
            {
                if (keyValueColumn.Key != EntityMap.PrimaryKeyName)
                {
                    columnNames += keyValueColumn.Value + ", ";
                    values += "{" + keyValueColumn.Key + "}, ";
                }
                else
                {
                    if (!EntityMap.IsAutoincrement)
                    {
                        columnNames += keyValueColumn.Value + ", ";
                        values += "{" + keyValueColumn.Key + "}, ";
                    }
                }
            }
            foreach (var keyValue in EntityMap.ForeignKeys)
            {
                columnNames += keyValue.Value + ", ";
                values += "{" + keyValue.Key + "}, ";
            }
            if (string.IsNullOrEmpty(EntityMap.PrimaryKeyName))
            {
                foreach (var keyValue in EntityMap.Keys)
                {
                    if (!EntityMap.ColumnNames.ContainsKey(keyValue.Key))
                    {
                        columnNames += keyValue.Value + ", ";
                        values += "{" + keyValue.Key + "}, ";
                    }
                    else
                    {

                    }
                }
            }
            columnNames += "-";
            columnNames = columnNames.Replace(", -", "");
            values += "-";
            values = values.Replace(", -", "");
            Query = "INSERT INTO " + EntityMap.TableName + " (" + columnNames + ") VALUES (" + values + ");";
        }

        public string Insert(T entity)
        {
            try
            {
                string query = Query;
                foreach (var keyValueColumn in EntityMap.ColumnNames)
                {
                    var value = EntityMap.Type.GetProperty(keyValueColumn.Key).GetValue(entity);
                    query = query.Replace("{" + keyValueColumn.Key + "}", DataFormater.ParseToSQL(value));
                }

                if (string.IsNullOrEmpty(EntityMap.PrimaryKeyName))
                {
                    foreach (var keyValue in EntityMap.Entities)
                    {
                        IEntityMap entityMap = keyValue.Value;
                        object obj = EntityMap.Type.GetProperty(keyValue.Key).GetValue(entity);
                        if (obj != null)
                        {
                            if (string.IsNullOrEmpty(entityMap.PrimaryKeyName))
                            {
                                foreach (var keyValueEntity in entityMap.Entities)
                                {
                                    IEntityMap foreignEntity = keyValueEntity.Value;
                                    object o = entityMap.Type.GetProperty(keyValueEntity.Key).GetValue(obj);
                                    if (o != null)
                                    {
                                        if (string.IsNullOrEmpty(foreignEntity.PrimaryKeyName))
                                        {
                                            
                                        }
                                        else
                                        {
                                            if (foreignEntity.Type.IsEnum)
                                            {
                                                if (Enum.IsDefined(foreignEntity.Type, o))
                                                {
                                                    Enum @enum = (Enum)Enum.Parse(foreignEntity.Type, o + "");
                                                    query = query.Replace("{" + keyValue.Key + "." + keyValueEntity.Key + "}", DataFormater.ParseToSQL(@enum));
                                                }
                                                else
                                                {
                                                    throw new Exception("El tipo enum [" + foreignEntity.Type + "] no tiene definido el valor [" + o + "].");
                                                }
                                            }
                                            else
                                            {
                                                var value = foreignEntity.Type.GetProperty(foreignEntity.PrimaryKeyName).GetValue(o);
                                                query = query.Replace("{" + keyValue.Key + "." + keyValueEntity.Key + "}", DataFormater.ParseToSQL(value));
                                            }
                                        }
                                    }
                                }
                                foreach (var keyValueColumn in entityMap.Keys)
                                {
                                    var value = entityMap.Type.GetProperty(keyValueColumn.Key).GetValue(obj);
                                    query = query.Replace("{" + keyValueColumn.Key + "}", DataFormater.ParseToSQL(value));
                                }
                            }
                            else
                            {
                                var value = entityMap.Type.GetProperty(entityMap.PrimaryKeyName).GetValue(obj);
                                query = query.Replace("{" + keyValue.Key + "}", DataFormater.ParseToSQL(value));
                            }
                        }
                    }
                }
                else
                {
                    foreach (var keyValuePair in EntityMap.Entities)
                    {
                        IEntityMap entityMap = keyValuePair.Value;
                        if (string.IsNullOrEmpty(entityMap.PrimaryKeyName))
                        {
                            object obj = EntityMap.Type.GetProperty(keyValuePair.Key).GetValue(entity);
                            if (obj != null)
                            {
                                foreach (var keyValue in entityMap.Entities)
                                {
                                    IEntityMap foreignKeyEntity = keyValue.Value;
                                    if (foreignKeyEntity.Type.IsEnum)
                                    {
                                        if (Enum.IsDefined(foreignKeyEntity.Type, obj))
                                        {
                                            Enum @enum = (Enum)Enum.Parse(foreignKeyEntity.Type, obj + "");
                                            query = query.Replace("{" + keyValuePair.Key + "." + keyValue.Key + "}", DataFormater.ParseToSQL(@enum));
                                        }
                                        else
                                        {
                                            throw new Exception("El tipo enum [" + foreignKeyEntity.Type + "] no tiene definido el valor [" + obj + "].");
                                        }
                                    }
                                    else
                                    {
                                        object o = entityMap.Type.GetProperty(keyValue.Key).GetValue(obj);
                                        var value = foreignKeyEntity.Type.GetProperty(foreignKeyEntity.PrimaryKeyName).GetValue(o);
                                        query = query.Replace("{" + keyValuePair.Key + "." + keyValue.Key + "}", DataFormater.ParseToSQL(value));
                                    }
                                }
                                foreach (var keyValue in entityMap.Keys)
                                {
                                    var value = entityMap.Type.GetProperty(keyValue.Key).GetValue(obj);
                                    query = query.Replace("{" + keyValuePair.Key + "." + keyValue.Key + "}", DataFormater.ParseToSQL(value));
                                }
                            }
                        }
                        else
                        {
                            object obj = EntityMap.Type.GetProperty(keyValuePair.Key).GetValue(entity);
                            if (obj != null)
                            {
                                if (entityMap.Type.IsEnum)
                                {
                                    if (Enum.IsDefined(entityMap.Type, obj))
                                    {
                                        Enum @enum = (Enum)Enum.Parse(entityMap.Type, obj + "");
                                        query = query.Replace("{" + keyValuePair.Key + "}", DataFormater.ParseToSQL(@enum));
                                    }
                                    else
                                    {
                                        throw new Exception("El tipo enum [" + keyValuePair.Value.Type + "] no tiene definido el valor [" + obj + "].");
                                    }
                                }
                                else
                                {
                                    var value = entityMap.Type.GetProperty(entityMap.PrimaryKeyName).GetValue(obj);
                                    query = query.Replace("{" + keyValuePair.Key + "}", DataFormater.ParseToSQL(value));
                                }
                            }
                        }
                    }
                }
                return query;
            }
            catch
            {
                throw;
            }
        }
    }
}
