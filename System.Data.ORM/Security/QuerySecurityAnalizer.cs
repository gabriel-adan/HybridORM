using System.Text.RegularExpressions;

namespace System.Data.ORM.Security
{
    internal static class QuerySecurityAnalizer
    {
        private static string NewLine = Environment.NewLine;
        private const string PATTERN = @"\'\b(?<val>\w+)\b\'\s*\={1}\s*\'\b(?<val>\w+)\b\'|\b(?<val>\w+)\s*\={1}?\s*(\k<val>)\b|\;\s*\b(?<exp>\w+)\b";

        public static string AnalizeQuery(string value)
        {
            if (!string.IsNullOrEmpty(value))
            {
                MatchCollection matches = Regex.Matches(value, PATTERN);
                if (matches.Count > 0)
                {
                    string message = "Sql injection in " + value + NewLine;
                    foreach (Match match in matches)
                    {
                        message += "malicious statement [" + match.Value + "] " + NewLine;
                    }
                    throw new SQLSecurityException("SQL Security, malicious code", new Exception(message));
                }
            }
            return value;
        }
    }
}
