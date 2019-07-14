using System.Collections.Generic;
using System.Linq.Expressions;

namespace System.Data.ORM.Mapping
{
    public interface IToManyMap<T> where T : class
    {
        void ColumnName(string columnName);

        IToManyMap<T> HasMany<E>(Expression<Func<T, IList<E>>> expression);
    }
}
