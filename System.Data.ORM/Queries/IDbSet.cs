using System.Linq.Expressions;

namespace System.Data.ORM.Queries
{
    public interface IDbSet<T> : ICriteria<T> where T : class
    {
        T Find(object id);

        ICriteria<T> Where(Expression<Func<T, bool>> expression);

        bool Save(T entity);

        bool Update(T entity);

        bool Remove(T entity);
    }
}
