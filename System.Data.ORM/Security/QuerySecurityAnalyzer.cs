using System.Text.RegularExpressions;

namespace System.Data.ORM.Security
{
    internal class QuerySecurityAnalyzer
    {
        private static string NewLine = Environment.NewLine;
        private const string PATTERN = @"\'\b(?<val>\w+)\b\'\s*\={1}\s*\'\b(?<val>\w+)\b\'|\b(?<val>\w+)\s*\={1}?\s*(\k<val>)\b|\;\s*\b(?<exp>\w+)\b";

        public static string AnalyzeQuery(string query)
        {
            if (!string.IsNullOrEmpty(query))
            {
                MatchCollection matches = Regex.Matches(query, PATTERN);
                if (matches.Count > 0)
                {
                    string message = "SQL Injection in query [" + query + "]" + NewLine;
                    foreach (Match match in matches)
                        message += "malicious statement [" + match.Value + "]" + NewLine;

                    throw new SQLSecurityException(message);
                }
            }
            return query;
        }
    }
}
