using System.Linq.Expressions;

namespace System.Data.ORM.Mapping
{
    internal interface IEntityMapping<T> where T : class
    {
        void Table(string tableName);
        IPropertyIdMap Id<TId>(Expression<Func<T, TId>> expression);
        IPropertyMap Map<TId>(Expression<Func<T, TId>> expression);
        IForeignKeyMap<T> ForeignKey<E>(Expression<Func<T, E>> expression);
        IPropertyKeyMap Key<EKey>(Expression<Func<T, EKey>> expression);
    }
}
