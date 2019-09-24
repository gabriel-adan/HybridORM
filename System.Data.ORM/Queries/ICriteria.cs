using System.Linq.Expressions;

namespace System.Data.ORM.Queries
{
    public interface ICriteria<T> : IGroup<T> where T : class
    {
        T UniqueResult();

        new ICriteria<T> OrderBy<E>(Expression<Func<T, E>> expression, Order orderType);
    }
}
