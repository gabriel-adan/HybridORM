using System.Linq.Expressions;

namespace System.Data.ORM.Mapping
{
    internal class PropertyForeignKeyTranslator : ExpressionVisitor
    {
        string propertyName;

        public PropertyForeignKeyTranslator()
        {

        }

        public string Translate(Expression expression)
        {
            Visit(expression);
            return propertyName;
        }

        protected override Expression VisitMember(MemberExpression node)
        {
            string[] entityNames = node.ToString().Split('.');
            if (entityNames.Length == 3)
            {
                propertyName = entityNames[1] + "." + node.Member.Name;
            }
            else
            {
                propertyName = node.Member.Name;
            }

            return node;
        }
    }
}
