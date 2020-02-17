using System.Data.ORM.CoreMap;
using System.Collections.Generic;

namespace System.Data.ORM.Cfg
{
    internal static class Configuration
    {
        internal static IDictionary<string, IEntityMap> Mappings { get; }
        internal static IList<object> Sets;
        internal static IDictionary<string, IViewMap> ViewMappings { get; }
        internal static IList<object> Views;
        internal static IDictionary<string, IQueryMap> QueryMappings { get; }
        internal static IList<object> Queries;
        internal static ISQLConfiguration configuration;

        static Configuration()
        {
            Mappings = new Dictionary<string, IEntityMap>();
            Sets = new List<object>();
            ViewMappings = new Dictionary<string, IViewMap>();
            Views = new List<object>();
            QueryMappings = new Dictionary<string, IQueryMap>();
            Queries = new List<object>();
        }
    }
}
