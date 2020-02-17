using System.Collections.Generic;

namespace System.Data.ORM.Queries
{
    public interface IQuery<Q> where Q : class
    {
        IQuery<Q> SetParameterValue(string name, object value);

        IList<Q> Execute();
    }
}