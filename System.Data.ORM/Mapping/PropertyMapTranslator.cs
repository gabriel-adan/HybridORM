using System.Linq.Expressions;

namespace System.Data.ORM.Mapping
{
    internal class PropertyMapTranslator : ExpressionVisitor
    {
        private string fieldName;

        public PropertyMapTranslator()
        {

        }

        public string Translate(Expression expression)
        {
            Visit(expression);
            return fieldName;
        }

        protected override Expression VisitMember(MemberExpression node)
        {
            fieldName = node.Member.Name;
            return node;
        }
    }
}
