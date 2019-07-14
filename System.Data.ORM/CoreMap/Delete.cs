using System.Data.ORM.Contracts;

namespace System.Data.ORM.CoreMap
{
    internal class Delete<T> : IDelete<T> where T : class
    {
        string query;
        IEntityMap EntityMap;

        public Delete(IEntityMap entityMap)
        {
            EntityMap = entityMap;
            query = "DELETE FROM " + entityMap.TableName + " WHERE ";
            if (string.IsNullOrEmpty(entityMap.PrimaryKeyName) && entityMap.ForeignKeys.Count > 0)
            {
                string foreignKeyName;
                foreach (var keyValue in entityMap.Entities)
                {
                    entityMap.ForeignKeys.TryGetValue(keyValue.Key, out foreignKeyName);
                    query += foreignKeyName + " = {" + keyValue.Key + "} AND ";
                }
                query += "-";
                query = query.Replace(" AND -", ";");
            }
            else
            {
                query += entityMap.PrimaryKeyName + " = {Id};";
            }
        }

        public string Remove(T entity)
        {
            string sql = query;
            if (string.IsNullOrEmpty(EntityMap.PrimaryKeyName))
            {
                foreach (var keyValue in EntityMap.Entities)
                {
                    IEntityMap entityMap = keyValue.Value;
                    object obj = EntityMap.Type.GetProperty(keyValue.Key).GetValue(entity);
                    if (obj != null)
                    {
                        if (!string.IsNullOrEmpty(entityMap.PrimaryKeyName))
                        {
                            var value = entityMap.Type.GetProperty(entityMap.PrimaryKeyName).GetValue(obj);
                            sql = sql.Replace("{" + keyValue.Key + "}", DataFormater.ParseToSQL(value));
                        }
                    }
                }
            }
            else
            {
                var value = EntityMap.Type.GetProperty(EntityMap.PrimaryKeyName).GetValue(entity);
                sql = sql.Replace("{Id}", DataFormater.ParseToSQL(value));
            }
            return sql;
        }
    }
}
