using System.Data.ORM.Cfg;
using System.Data.ORM.CoreMap;
using System.Data.ORM.Queries;
using System.Data.ORM.Security;
using System.Reflection;
using System.Collections.Generic;

namespace System.Data.ORM.Context
{
    internal class Query<Q> : IQuery<Q> where Q : class
    {
        IDbConnection connection;
        ISQLConfiguration configuration;
        Assembly assembly;
        IQueryMap queryMap;

        public Query(IDbConnection connection, ISQLConfiguration configuration, Assembly assembly, IQueryMap queryMap)
        {
            this.connection = connection;
            this.configuration = configuration;
            this.assembly = assembly;
            this.queryMap = queryMap;
        }

        public IQuery<Q> SetParameterValue(string name, object value)
        {
            queryMap.CurrentQuery = queryMap.CurrentQuery.Replace(name, DataFormater.ParseToSQL(value));
            return this;
        }

        public IList<Q> Execute()
        {
            try
            {
                IList<Q> list = new List<Q>();
                string query = queryMap.CurrentQuery;
                queryMap.CurrentQuery = queryMap.Query;
                using (IDbCommand command = CreateCommand(query))
                {
                    using (IDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            Q entity = Activator.CreateInstance(queryMap.Type) as Q;
                            foreach (var keyValueColumn in queryMap.ColumnNames)
                            {
                                var value = reader[keyValueColumn.Key];
                                PropertyInfo property = queryMap.Type.GetProperty(keyValueColumn.Key);
                                property.SetValue(entity, DataFormater.ParseToData(property, value));
                            }
                            list.Add(entity);
                        }
                        reader.Close();
                        reader.Dispose();
                        command.Dispose();
                    }
                }
                return list;
            }
            catch
            {
                throw;
            }
        }

        IDbCommand CreateCommand(string query)
        {
            query = QuerySecurityAnalyzer.AnalyzeQuery(query);
            if (configuration.IsShowSql)
                Diagnostics.Debug.WriteLine("Query Executed ===> [" + query + "] ");
            IDbCommand command = assembly.CreateInstance(configuration.CommandTypeName(), true) as IDbCommand;
            command.Connection = connection as IDbConnection;
            command.CommandText = query;
            return command;
        }
    }
}