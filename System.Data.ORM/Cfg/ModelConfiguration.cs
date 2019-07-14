﻿using System.Reflection;
using System.Data.ORM.CoreMap;
using System.Data.ORM.Mapping;

namespace System.Data.ORM.Cfg
{
    public class ModelConfiguration
    {
        private Assembly AssemblyModel;

        public ModelConfiguration(string assemblyName, string folderName)
        {
            try
            {
                AssemblyModel = Assembly.Load(assemblyName);
                foreach (Type type in AssemblyModel.ExportedTypes)
                {
                    if (type.Namespace.Equals(assemblyName + "." + folderName))
                    {
                        IEntityMap entityMap = Activator.CreateInstance(typeof(EntityMap<>).MakeGenericType(type)) as IEntityMap;
                        Configuration.Mappings.Add(type.Name, entityMap);
                    }
                }
            }
            catch
            {
                throw;
            }
        }

        public ModelConfiguration Add<T>() where T : class
        {
            try
            {
                Type type = typeof(T);
                if (type.BaseType.Name != typeof(ClassMap<>).Name)
                    throw new Exception("El tipo [" + type + "] debe ser una clase heredada del tipo [" + typeof(ClassMap<>) + "]");
                string entityKey = type.BaseType.GetGenericArguments()[0].Name;
                Configuration.Mappings.Remove(entityKey);
                IEntityMap entityMap = Activator.CreateInstance(type) as IEntityMap;
                Configuration.Mappings.Add(entityKey, entityMap);
                return this;
            }
            catch
            {
                throw;
            }
        }
    }
}
