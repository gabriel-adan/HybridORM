using System.Linq.Expressions;
using System.Collections.Generic;

namespace System.Data.ORM.Queries
{
    public interface IOrder<T> where T : class
    {
        IList<T> OrderBy<E>(Expression<Func<T, E>> expression);

        IList<T> OrderBy<E>(Expression<Func<T, E>> expression, Order orderType);

        IList<T> ToList();
    }
}
