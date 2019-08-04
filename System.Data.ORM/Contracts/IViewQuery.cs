using System.Data.ORM.CoreMap;
using System.Linq.Expressions;

namespace System.Data.ORM.Contracts
{
    internal interface IViewQuery<V> where V : class
    {
        IViewMap ViewMap { get; }

        string ToList();

        string Where<E>(Expression<Func<V, E>> expression);

        string GroupBy<E>(Expression<Func<V, E>> expression);

        string OrderBy<E>(Expression<Func<V, E>> expression);
    }
}
