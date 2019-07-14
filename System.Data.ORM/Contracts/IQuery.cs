using System.Data.ORM.CoreMap;
using System.Linq.Expressions;

namespace System.Data.ORM.Contracts
{
    internal interface IQuery<T> where T : class
    {
        IEntityMap EntityMap { get; }

        string Find(object id);

        string Where(Expression<Func<T, bool>> expression);

        string ToList();

        string GroupBy<E>(Expression<Func<T, E>> expression);

        string OrderBy<E>(Expression<Func<T, E>> expression);
    }
}
