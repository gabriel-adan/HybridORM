using System.Data.ORM.Mapping;
using System.Linq.Expressions;

namespace System.Data.ORM.CoreMap
{
    internal interface IViewMapping<V> where V : class
    {
        IPropertyMap Map<E>(Expression<Func<V, E>> expression);
    }
}
