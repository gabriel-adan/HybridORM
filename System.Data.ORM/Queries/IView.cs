using System.Linq.Expressions;

namespace System.Data.ORM.Queries
{
    public interface IView<V> : ICriteria<V> where V : class
    {
        ICriteria<V> Where(Expression<Func<V, bool>> expression);
    }
}
