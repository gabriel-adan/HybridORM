using System.Data.ORM.Contracts;

namespace System.Data.ORM.CoreMap
{
    public class Modify<T> : IModify<T> where T : class
    {
        string query;
        IEntityMap EntityMap;

        public Modify(IEntityMap entityMap)
        {
            EntityMap = entityMap;
            query = "UPDATE " + entityMap.TableName + " SET ";
            foreach (var keyValueColumn in entityMap.ColumnNames)
            {
                if (keyValueColumn.Key != entityMap.PrimaryKeyName)
                {
                    query += keyValueColumn.Value + " = {" + keyValueColumn.Key + "}, ";
                }
                else
                {
                    if (!entityMap.IsAutoincrement)
                    {
                        query += keyValueColumn.Value + " = {" + keyValueColumn.Key + "}, ";
                    }
                }
            }
            query += "-";
            query = query.Replace(", -", " ");
            query += "WHERE ";
            if (string.IsNullOrEmpty(entityMap.PrimaryKeyName))
            {
                foreach (var keyValue in entityMap.ForeignKeys)
                {
                    query += keyValue.Value + " = {" + keyValue.Key + "} AND ";
                }
                foreach (var keyValue in entityMap.Keys)
                {
                    query += keyValue.Value + " = {" + keyValue.Key + "} AND ";
                }
                query += "-";
                query = query.Replace(" AND -", ";");
            }
            else
            {
                if (entityMap.IsAutoincrement)
                {
                    query += entityMap.PrimaryKeyName + " = {Id};";
                }
                else
                {
                    foreach (var keyValue in entityMap.ForeignKeys)
                    {
                        query += keyValue.Value + " = {" + keyValue.Key + "} AND ";
                    }
                    foreach (var keyValue in entityMap.Keys)
                    {
                        query += keyValue.Value + " = {" + keyValue.Key + "} AND ";
                    }
                    query += entityMap.PrimaryKeyName + " = {Id};";
                }
            }
        }

        public string Update(T entity)
        {
            string sql = query;
            foreach (var keyValueColumn in EntityMap.ColumnNames)
            {
                var value = EntityMap.Type.GetProperty(keyValueColumn.Key).GetValue(entity);
                sql = sql.Replace("{" + keyValueColumn.Key + "}", DataFormater.ParseToSQL(value));
            }
            foreach (var keyValue in EntityMap.Entities)
            {
                IEntityMap entityMap = keyValue.Value;
                object obj = EntityMap.Type.GetProperty(keyValue.Key).GetValue(entity);
                if (obj != null)
                {
                    if (!string.IsNullOrEmpty(entityMap.PrimaryKeyName))
                    {
                        object fk = entityMap.Type.GetProperty(entityMap.PrimaryKeyName).GetValue(obj);
                        sql = sql.Replace("{" + keyValue.Key + "}", DataFormater.ParseToSQL(fk));
                    }
                }
            }
            return sql;
        }
    }
}
