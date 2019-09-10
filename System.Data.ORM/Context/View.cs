using System.Reflection;
using System.Data.ORM.Cfg;
using System.Data.ORM.Queries;
using System.Linq.Expressions;
using System.Collections.Generic;
using System.Data.ORM.CoreMap;
using System.Data.ORM.Contracts;

namespace System.Data.ORM.Context
{
    internal class View<V> : IView<V> where V : class
    {
        IDbConnection connection;
        ISQLConfiguration configuration;
        Assembly assembly;
        IViewQuery<V> viewQuery;
        string currentQuery;

        public View(IDbConnection connection, ISQLConfiguration configuration, Assembly assembly, IViewQuery<V> viewQuery)
        {
            this.connection = connection;
            this.configuration = configuration;
            this.assembly = assembly;
            this.viewQuery = viewQuery;
        }

        public ICriteria<V> Where(Expression<Func<V, bool>> expression)
        {
            try
            {
                currentQuery = viewQuery.ViewMap.Query + viewQuery.Where(expression);
                return this;
            }
            catch
            {
                throw;
            }
        }

        public IOrder<V> GroupBy<E>(Expression<Func<V, E>> expression)
        {
            try
            {
                string groupBy = viewQuery.GroupBy(expression);
                if (string.IsNullOrEmpty(currentQuery))
                {
                    currentQuery = viewQuery.ViewMap.Query + " " + groupBy;
                }
                else
                {
                    currentQuery += groupBy;
                }
                return this;
            }
            catch
            {
                throw;
            }
        }

        public IList<V> OrderBy<E>(Expression<Func<V, E>> expression)
        {
            try
            {
                if (string.IsNullOrEmpty(currentQuery))
                    currentQuery = viewQuery.ViewMap.Query + viewQuery.OrderBy(expression);
                else
                    currentQuery += viewQuery.OrderBy(expression);
                string sql = currentQuery;
                currentQuery = null;
                return Execute(sql);
            }
            catch
            {
                throw;
            }
        }

        public IList<V> OrderBy<E>(Expression<Func<V, E>> expression, Order orderType)
        {
            try
            {
                if (string.IsNullOrEmpty(currentQuery))
                    currentQuery = viewQuery.ViewMap.Query + viewQuery.OrderBy(expression) + " " + orderType + ";";
                else
                    currentQuery += viewQuery.OrderBy(expression) + " " + orderType + ";";
                string sql = currentQuery;
                currentQuery = null;
                return Execute(sql);
            }
            catch
            {
                throw;
            }
        }

        public IList<V> ToList()
        {
            try
            {
                if (string.IsNullOrEmpty(currentQuery))
                    currentQuery = viewQuery.ToList();
                string sql = currentQuery;
                currentQuery = null;
                return Execute(sql);
            }
            catch
            {
                throw;
            }
        }

        public V UniqueResult()
        {
            V entity = default(V);
            try
            {
                if (string.IsNullOrEmpty(currentQuery))
                    currentQuery = viewQuery.ViewMap.Query + " LIMIT 1;";
                else
                    currentQuery += " LIMIT 1;";
                string sql = currentQuery;
                currentQuery = null;
                using (IDbCommand command = CreateCommand(sql))
                {
                    using (IDataReader reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            entity  = Activator.CreateInstance(viewQuery.ViewMap.Type) as V;
                            foreach (var keyValueColumn in viewQuery.ViewMap.ColumnNames)
                            {
                                var value = reader[keyValueColumn.Key];
                                PropertyInfo property = viewQuery.ViewMap.Type.GetProperty(keyValueColumn.Key);
                                property.SetValue(entity, DataFormater.ParseToData(property, value));
                            }
                        }
                        reader.Close();
                        reader.Dispose();
                        command.Dispose();
                    }
                }
                return entity;
            }
            catch
            {
                throw;
            }
        }

        IList<V> Execute(string sql)
        {
            try
            {
                IList<V> list = Activator.CreateInstance(typeof(List<>).MakeGenericType(viewQuery.ViewMap.Type)) as IList<V>;
                using (IDbCommand command = CreateCommand(sql))
                {
                    using (IDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            V entity = Activator.CreateInstance(viewQuery.ViewMap.Type) as V;
                            foreach (var keyValueColumn in viewQuery.ViewMap.ColumnNames)
                            {
                                var value = reader[keyValueColumn.Key];
                                PropertyInfo property = viewQuery.ViewMap.Type.GetProperty(keyValueColumn.Key);
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
            if (configuration.IsShowSql)
                Diagnostics.Debug.WriteLine("Query Executed ===> [" + query + "] ");
            IDbCommand command = assembly.CreateInstance(configuration.CommandTypeName(), true) as IDbCommand;
            command.Connection = connection as IDbConnection;
            command.CommandText = query;
            return command;
        }
    }
}
