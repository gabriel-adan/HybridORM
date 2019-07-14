namespace System.Data.ORM.Queries
{
    public interface ICriteria<T> : IGroup<T> where T : class
    {
        T UniqueResult();
    }
}
