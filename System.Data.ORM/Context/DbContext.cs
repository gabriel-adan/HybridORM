using System.Linq;
using System.Reflection;
using System.Collections.Generic;
using System.Data.ORM.Cfg;
using System.Data.ORM.CoreMap;
using System.Data.ORM.Queries;

namespace System.Data.ORM.Context
{
    public abstract class DbContext : IDisposable
    {
        private IDbConnection connection;
        private IDbTransaction transaction;
        private readonly Assembly assembly;

        public DbContext(ISQLConfiguration configuration)
        {
            Cfg.Configuration.configuration = configuration;
            try
            {
                assembly = Assembly.Load(configuration.AssemblyTypeName());
                connection = assembly.CreateInstance(configuration.ConnectionTypeName()) as IDbConnection;
                connection.ConnectionString = configuration.ConnectionString;
                foreach (var mapping in Cfg.Configuration.Mappings)
                {
                    foreach (PropertyInfo property in GetEntities(mapping.Value.Type))
                    {
                        IEntityMap entityMap;
                        Cfg.Configuration.Mappings.TryGetValue(property.PropertyType.Name, out entityMap);
                        mapping.Value.Entities.Add(property.Name, entityMap);
                    }
                    foreach (PropertyInfo property in GetCollections(mapping.Value.Type))
                    {
                        Type itemType = property.PropertyType.GetGenericArguments()[0];
                        IEntityMap itemEntityMap;
                        Cfg.Configuration.Mappings.TryGetValue(itemType.Name, out itemEntityMap);
                        mapping.Value.Collections.Add(property.Name, itemEntityMap);
                    }
                }
                foreach (var mapping in Cfg.Configuration.Mappings)
                {
                    if (!mapping.Value.Type.IsEnum)
                    {
                        var query = Activator.CreateInstance(typeof(CoreMap.Query<>).MakeGenericType(mapping.Value.Type), mapping.Value);
                        var persist = Activator.CreateInstance(typeof(Persist<>).MakeGenericType(mapping.Value.Type), mapping.Value);
                        var modify = Activator.CreateInstance(typeof(Modify<>).MakeGenericType(mapping.Value.Type), mapping.Value);
                        var delete = Activator.CreateInstance(typeof(Delete<>).MakeGenericType(mapping.Value.Type), mapping.Value);
                        var set = Activator.CreateInstance(typeof(Set<>).MakeGenericType(mapping.Value.Type), connection, configuration, assembly, query, persist, modify, delete);
                        Cfg.Configuration.Sets.Add(set);
                    }
                }
                foreach (var mapping in Cfg.Configuration.ViewMappings)
                {
                    var viewQuery = Activator.CreateInstance(typeof(ViewQuery<>).MakeGenericType(mapping.Value.Type), mapping.Value);
                    var view = Activator.CreateInstance(typeof(View<>).MakeGenericType(mapping.Value.Type), connection, configuration, assembly, viewQuery);
                    Cfg.Configuration.Views.Add(view);
                }
                foreach(var mapping in Cfg.Configuration.QueryMappings)
                {
                    var queryMap = Activator.CreateInstance(typeof(Mapping.QueryMap<>).MakeGenericType(mapping.Value.Type), mapping.Value.Query);
                    var query = Activator.CreateInstance(typeof(Query<>).MakeGenericType(mapping.Value.Type), connection, configuration, assembly, queryMap);
                    Cfg.Configuration.Queries.Add(query);
                }
            }
            catch
            {
                throw;
            }
        }

        public IDbSet<T> Set<T>() where T : class
        {
            Type type = typeof(T);
            IDbSet<T> DbSet = Cfg.Configuration.Sets.Where(set => set.GetType().GetGenericArguments()[0] == type).FirstOrDefault() as IDbSet<T>;
            if (DbSet == null)
                throw new Exception("Entity of type: [" + type + "] not mapped.");
            return DbSet;
        }

        public IView<V> View<V>() where V : class
        {
            Type type = typeof(V);
            IView<V> view = Cfg.Configuration.Views.Where(v => v.GetType().GetGenericArguments()[0] == type).FirstOrDefault() as IView<V>;
            if (view == null)
                throw new Exception("Entity of type: [" + type + "] not mapped.");
            return view;
        }

        public IQuery<Q> Query<Q>() where Q : class
        {
            Type type = typeof(Q);
            IQuery<Q> query = Cfg.Configuration.Queries.Where(q => q.GetType().GetGenericArguments()[0] == type).FirstOrDefault() as IQuery<Q>;
            if (query == null)
                throw new Exception("Entity of type: [" + type + "] not mapped.");
            return query;
        }

        IList<PropertyInfo> GetEntities(Type type)
        {
            IList<PropertyInfo> properties = new List<PropertyInfo>();
            foreach (var property in type.GetProperties())
            {
                if (!property.PropertyType.Namespace.Equals("System") && !property.PropertyType.Namespace.Equals("System.Collections.Generic"))
                    properties.Add(property);
            }
            return properties;
        }

        IList<PropertyInfo> GetCollections(Type type)
        {
            IList<PropertyInfo> collections = new List<PropertyInfo>();
            foreach (var property in type.GetProperties())
            {
                if (property.PropertyType.Namespace.Equals("System.Collections.Generic"))
                    collections.Add(property);
            }
            return collections;
        }

        public bool OpenConnection()
        {
            try
            {
                connection.Open();
                return true;
            }
            catch
            {
                throw;
            }
        }

        public bool CloseConnection()
        {
            try
            {
                connection.Close();
                return true;
            }
            catch
            {
                throw;
            }
        }

        public void BeginTransaction()
        {
            try
            {
                transaction = connection.BeginTransaction();
            }
            catch
            {
                throw;
            }
        }

        public void CommitTransaction()
        {
            try
            {
                if (transaction != null)
                    transaction.Commit();
            }
            catch
            {
                throw;
            }
        }

        public void RollBackTransaction()
        {
            try
            {
                if (transaction != null)
                    transaction.Rollback();
            }
            catch
            {
                throw;
            }
        }

        public void Dispose()
        {
            try
            {
                if (transaction != null)
                    transaction.Dispose();
                if (connection != null)
                {
                    if (connection.State == ConnectionState.Open)
                        connection.Close();
                    connection.Dispose();
                }
            }
            catch
            {
                throw;
            }
        }
    }
}
