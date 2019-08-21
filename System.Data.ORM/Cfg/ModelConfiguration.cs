using System.Reflection;
using System.Data.ORM.CoreMap;
using System.Data.ORM.Mapping;

namespace System.Data.ORM.Cfg
{
    public class ModelConfiguration
    {
        public ModelConfiguration(string assemblyName, string folderName)
        {
            try
            {
                var assemblyModel = Assembly.Load(assemblyName);
                foreach (Type type in assemblyModel.ExportedTypes)
                {
                    if (type.Namespace.Equals(assemblyName + "." + folderName))
                    {
                        if (!type.IsEnum)
                        {
                            IEntityMap entityMap = Activator.CreateInstance(typeof(EntityMap<>).MakeGenericType(type)) as IEntityMap;
                            Configuration.Mappings.Add(type.Name, entityMap);
                        }
                        else
                        {
                            IEnumMap enumMap = Activator.CreateInstance(typeof(EnumMap), type) as IEnumMap;
                            Configuration.Mappings.Add(type.Name, enumMap);
                        }
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

        public ModelConfiguration View<V>()
        {
            try
            {
                Type type = typeof(V);
                if (type.BaseType.Name != typeof(ViewMap<>).Name)
                    throw new Exception("El tipo [" + type + "] debe ser una clase heredada del tipo [" + typeof(ViewMap<>) + "]");
                string entityKey = type.BaseType.GetGenericArguments()[0].Name;
                Configuration.Mappings.Remove(entityKey);
                IViewMap viewMap = Activator.CreateInstance(type) as IViewMap;
                Configuration.ViewMappings.Add(entityKey, viewMap);
                return this;
            }
            catch
            {
                throw;
            }
        }
    }
}
