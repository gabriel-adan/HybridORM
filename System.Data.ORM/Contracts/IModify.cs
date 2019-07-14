namespace System.Data.ORM.Contracts
{
    internal interface IModify<T> where T : class
    {
        string Update(T entity);
    }
}
