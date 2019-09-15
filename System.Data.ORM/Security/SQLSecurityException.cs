namespace System.Data.ORM.Security
{
    public class SQLSecurityException : Exception
    {
        public SQLSecurityException(string message) : base(message)
        {
            this.Source = "System.Data.ORM.Security";
        }

        public SQLSecurityException (string message, Exception inner) : base(message, inner)
        {
            this.Source = "System.Data.ORM.Security";
        }
    }
}
