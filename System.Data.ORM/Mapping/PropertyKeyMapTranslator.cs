using System.Linq.Expressions;

namespace System.Data.ORM.Mapping
{
    internal class PropertyKeyMapTranslator : ExpressionVisitor
    {
        string propertyName;

        public PropertyKeyMapTranslator()
        {

        }

        public string Translate(Expression expression)
        {
            Visit(expression);
            return propertyName;
        }

        protected override Expression VisitMember(MemberExpression node)
        {
            propertyName = node.Member.Name;
            return node;
        }
    }
}
