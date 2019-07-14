namespace System.Data.ORM.Contracts
{
    internal interface IPersist<T> where T : class
    {
        string Insert(T entity);
    }
}
