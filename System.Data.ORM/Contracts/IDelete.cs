namespace System.Data.ORM.Contracts
{
    internal interface IDelete<T> where T : class
    {
        string Remove(T entity);
    }
}
