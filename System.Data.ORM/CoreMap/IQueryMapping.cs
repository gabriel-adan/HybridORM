using System.Data.ORM.Mapping;
using System.Linq.Expressions;

namespace System.Data.ORM.CoreMap
{
    internal interface IQueryMapping<Q> where Q : class
    {
        IPropertyMap Map<E>(Expression<Func<Q, E>> expression);
    }
}