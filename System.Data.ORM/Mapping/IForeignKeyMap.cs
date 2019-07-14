using System.Linq.Expressions;

namespace System.Data.ORM.Mapping
{
    public interface IForeignKeyMap<T> where T : class
    {
        void ColumnName(string columnName);

        IForeignKeyMap<T> ForeignKey<E>(Expression<Func<T, E>> expression);
    }
}
