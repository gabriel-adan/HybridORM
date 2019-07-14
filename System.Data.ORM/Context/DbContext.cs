﻿using System.Linq;
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
                    var query = Activator.CreateInstance(typeof(Query<>).MakeGenericType(mapping.Value.Type), mapping.Value);
                    var persist = Activator.CreateInstance(typeof(Persist<>).MakeGenericType(mapping.Value.Type), mapping.Value);
                    var modify = Activator.CreateInstance(typeof(Modify<>).MakeGenericType(mapping.Value.Type), mapping.Value);
                    var delete = Activator.CreateInstance(typeof(Delete<>).MakeGenericType(mapping.Value.Type), mapping.Value);
                    var set = Activator.CreateInstance(typeof(Set<>).MakeGenericType(mapping.Value.Type), connection, configuration, assembly, query, persist, modify, delete);
                    Cfg.Configuration.Sets.Add(set);
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
                throw new Exception("Entity of type: [" + type + "] not mapping.");
            return DbSet;
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