using System.Linq.Expressions;
using System.Data.ORM.Contracts;
using System.Linq;

namespace System.Data.ORM.CoreMap
{
    internal class Query<T> : IQuery<T> where T : class
    {
        public IEntityMap EntityMap { get; }
        readonly QueryTranslator<T> QueryTranslator;

        public Query(IEntityMap entityMap)
        {
            QueryTranslator = new QueryTranslator<T>(entityMap);
            EntityMap = entityMap;
            BuidQuery();
            BuildCollectionsQuery();
        }

        public string Find(object id)
        {
            if (string.IsNullOrEmpty(EntityMap.PrimaryKeyName))
                throw new Exception("Entity [" + EntityMap.Type + "] without primary key mapped.");
            return EntityMap.Select + "WHERE _this." + EntityMap.PrimaryKeyName + " = " + DataFormater.ParseToSQL(id) + ";";
        }

        void BuidQuery()
        {
            string columnNames = string.Empty, foreignColumnNames = string.Empty, joins = string.Empty;
            foreach (var keyValueColumn in EntityMap.ColumnNames)
                columnNames += "_this." + keyValueColumn.Value + " AS '_this." + keyValueColumn.Key + "', ";

            foreach (var keyValuePair in EntityMap.Entities)
            {
                IEntityMap foreignEntity = keyValuePair.Value;
                foreach (var keyValue in foreignEntity.ColumnNames)
                    foreignColumnNames += keyValuePair.Key + "." + keyValue.Value + " AS '" + keyValuePair.Key + "." + keyValue.Key + "', ";

                var list = EntityMap.Entities.Where(e => e.Value.Type == foreignEntity.Type).ToList();
                bool isLeftJoin = list.Count > 1;

                string foreignKeyName;
                if (string.IsNullOrEmpty(foreignEntity.PrimaryKeyName) && foreignEntity.Entities.Count > 0)
                {
                    string join = "INNER JOIN " + foreignEntity.TableName + " " + keyValuePair.Key + " ON ";
                    
                    foreach (var keyValue in foreignEntity.Entities)
                    {
                        IEntityMap entity = keyValue.Value;
                        foreach (var keyValueColumn in entity.ColumnNames)
                            foreignColumnNames += keyValue.Key + "." + keyValueColumn.Value + " AS '" + keyValue.Key + "." + keyValueColumn.Key + "', ";
                        EntityMap.ForeignKeys.TryGetValue(keyValuePair.Key + "." + keyValue.Key, out foreignKeyName);
                        string fk;
                        if (string.IsNullOrEmpty(EntityMap.PrimaryKeyName))
                        {
                            foreignEntity.ForeignKeys.TryGetValue(keyValue.Key, out fk);
                            joins += "INNER JOIN " + entity.TableName + " " + keyValue.Key + " ON " + keyValuePair.Key + "." + fk + " = _this." + foreignKeyName + " ";
                            join += "_this." + foreignKeyName + " = " + keyValuePair.Key + "." + fk + " AND ";
                            
                            foreach (var keyValueFk in EntityMap.Keys)
                            {
                                join += "_this." + keyValueFk.Value + " = " + keyValuePair.Key + "." + keyValueFk.Key + " AND ";
                            }
                        }
                        else
                        {
                            foreignEntity.ForeignKeys.TryGetValue(keyValue.Key, out fk);
                            joins += "INNER JOIN " + entity.TableName + " " + keyValue.Key + " ON " + keyValuePair.Key + "." + fk + " = " + keyValue.Key + "." + entity.PrimaryKeyName + " ";
                            join += "_this." + foreignKeyName + " = " + keyValue.Key + "." + entity.PrimaryKeyName + " AND ";
                        }
                    }
                    join += "-";
                    join = join.Replace("AND -", "");
                    joins = join + joins;
                }
                else
                {
                    EntityMap.ForeignKeys.TryGetValue(keyValuePair.Key, out foreignKeyName);
                    if (isLeftJoin)
                        joins += "LEFT JOIN " + foreignEntity.TableName + " " + keyValuePair.Key + " ON (_this." + foreignKeyName + " = " + keyValuePair.Key + "." + foreignEntity.PrimaryKeyName + " OR _this." + foreignKeyName + " IS NULL) ";
                    else
                        joins += "INNER JOIN " + foreignEntity.TableName + " " + keyValuePair.Key + " ON _this." + foreignKeyName + " = " + keyValuePair.Key + "." + foreignEntity.PrimaryKeyName + " ";
                }
            }

            if (!string.IsNullOrEmpty(foreignColumnNames))
            {
                foreignColumnNames += "-";
                foreignColumnNames = foreignColumnNames.Replace(", -", "");
                EntityMap.ForeignSelect = "SELECT " + foreignColumnNames + " FROM " + EntityMap.TableName + " _this " + joins;
            }
            else
            {
                columnNames += "-";
                columnNames = columnNames.Replace(", -", "");
            }

            EntityMap.Select = "SELECT " + columnNames + foreignColumnNames + " FROM " + EntityMap.TableName + " _this " + joins;
        }

        void BuildCollectionsQuery()
        {
            foreach (var keyValuePair in EntityMap.Collections)
            {
                IEntityMap itemEntityMap = keyValuePair.Value;
                string columnNames = string.Empty, foreignColumnNames = string.Empty, joins = string.Empty, where = string.Empty;
                foreach (var keyValueColumn in itemEntityMap.ColumnNames)
                    columnNames += keyValuePair.Key + "." + keyValueColumn.Value + " AS '" + keyValuePair.Key + "." + keyValueColumn.Key + "', ";
                
                string entityForeignKeyName = string.Empty;
                where = "WHERE " + keyValuePair.Key;
                foreach (var keyValue in itemEntityMap.Entities)
                {
                    IEntityMap compositeEntity = keyValue.Value;
                    if (compositeEntity.Type != EntityMap.Type)
                    {
                        foreach (var keyValueColumn in compositeEntity.ColumnNames)
                            columnNames += keyValue.Key + "." + keyValueColumn.Value + " AS '" + keyValue.Key + "." + keyValueColumn.Key + "', ";

                        var list = itemEntityMap.Entities.Where(e => e.Value.Type == compositeEntity.Type).ToList();
                        bool isLeftJoin = list.Count > 1;

                        string foreignKeyName;
                        if (string.IsNullOrEmpty(compositeEntity.PrimaryKeyName))
                        {

                        }
                        else
                        {
                            itemEntityMap.ForeignKeys.TryGetValue(keyValue.Key, out foreignKeyName);
                            if (isLeftJoin)
                                joins += "LEFT JOIN " + compositeEntity.TableName + " " + keyValue.Key + " ON (" + keyValuePair.Key + "." + foreignKeyName + " = " + keyValue.Key + "." + compositeEntity.PrimaryKeyName + " OR " + keyValuePair.Key + "." + foreignKeyName + " IS NULL) ";
                            else
                                joins += "INNER JOIN " + compositeEntity.TableName + " " + keyValue.Key + " ON " + keyValuePair.Key + "." + foreignKeyName + " = " + keyValue.Key + "." + compositeEntity.PrimaryKeyName + " ";
                        }
                    }
                    else
                    {
                        itemEntityMap.ForeignKeys.TryGetValue(keyValue.Key, out entityForeignKeyName);
                        where += "." + entityForeignKeyName + " = {" + keyValue.Key + "} ";
                    }
                }
                columnNames += "-";
                columnNames = columnNames.Replace(", -", "");
                string groupBy = "GROUP BY " + keyValuePair.Key + "." + itemEntityMap.PrimaryKeyName + ";";
                if (string.IsNullOrEmpty(itemEntityMap.PrimaryKeyName))
                {
                    groupBy = "GROUP BY ";
                    foreach (var keyValue in itemEntityMap.ForeignKeys)
                    {
                        groupBy += keyValuePair.Key + "." + keyValue.Value + ", ";
                    }
                    groupBy += "-";
                    groupBy = groupBy.Replace(", -", ";");
                }
                string query = "SELECT " + columnNames + " FROM " + itemEntityMap.TableName + " " + keyValuePair.Key + " " + joins + where + groupBy;
                EntityMap.CollectionSelect.Add(keyValuePair.Key, query);
            }
        }

        public string Where(Expression<Func<T, bool>> expression)
        {
            return "WHERE " + QueryTranslator.Where(expression);
        }

        public string GroupBy<E>(Expression<Func<T, E>> expression)
        {
            return "GROUP BY " + QueryTranslator.GroupBy(expression);
        }

        public string OrderBy<E>(Expression<Func<T, E>> expression)
        {
            return "ORDER BY " + QueryTranslator.OrderBy(expression);
        }

        public string ToList()
        {
            return EntityMap.Select;
        }
    }
}
