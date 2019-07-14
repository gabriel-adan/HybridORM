using System.Linq.Expressions;

namespace System.Data.ORM.Queries
{
    public interface IGroup<T> : IOrder<T> where T : class
    {
        IOrder<T> GroupBy<E>(Expression<Func<T, E>> expression);
    }
}
