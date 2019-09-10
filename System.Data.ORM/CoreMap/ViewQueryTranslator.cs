using System.Linq.Expressions;
using System.Reflection;
using System.Text;

namespace System.Data.ORM.CoreMap
{
    internal class ViewQueryTranslator<V> : ExpressionVisitor
    {
        IViewMap ViewMap;
        StringBuilder _queryBuilder;

        public ViewQueryTranslator(IViewMap viewMap)
        {
            ViewMap = viewMap;
        }

        public string Where(Expression expression)
        {
            _queryBuilder = new StringBuilder();
            this.Visit(expression);
            return _queryBuilder.ToString();
        }

        public string GroupBy(Expression expression)
        {
            _queryBuilder = new StringBuilder();
            this.Visit(expression);
            return _queryBuilder.ToString();
        }

        public string OrderBy(Expression expression)
        {
            _queryBuilder = new StringBuilder();
            this.Visit(expression);
            return _queryBuilder.ToString();
        }

        protected override Expression VisitUnary(UnaryExpression node)
        {
            switch (node.NodeType)
            {
                case ExpressionType.Not:
                    _queryBuilder.Append(Cfg.Configuration.configuration.SQLNot() + " ");
                    this.Visit(node.Operand);
                    break;
                case ExpressionType.Convert:
                    this.Visit(node.Operand);
                    break;
                default:
                    throw new NotSupportedException(string.Format("El operador unario '{0}' no es soportado", node.NodeType));
            }

            return node;
        }

        protected override Expression VisitBinary(BinaryExpression node)
        {
            this.Visit(node.Left);
            switch (node.NodeType)
            {
                case ExpressionType.And:
                    _queryBuilder.Append(" AND ");
                    break;
                case ExpressionType.AndAlso:
                    _queryBuilder.Append(" AND ");
                    break;
                case ExpressionType.Or:
                    _queryBuilder.Append(" OR ");
                    break;
                case ExpressionType.OrElse:
                    _queryBuilder.Append(" OR ");
                    break;
                case ExpressionType.Equal:
                    _queryBuilder.Append(" = ");
                    break;
                case ExpressionType.NotEqual:
                    _queryBuilder.Append(" <> ");
                    break;
                case ExpressionType.LessThan:
                    _queryBuilder.Append(" < ");
                    break;
                case ExpressionType.LessThanOrEqual:
                    _queryBuilder.Append(" <= ");
                    break;
                case ExpressionType.GreaterThan:
                    _queryBuilder.Append(" > ");
                    break;
                case ExpressionType.GreaterThanOrEqual:
                    _queryBuilder.Append(" >= ");
                    break;
                default:
                    throw new NotSupportedException(string.Format("El operador binario '{0}' no es soportado", node.NodeType));
            }

            this.Visit(node.Right);
            return node;
        }

        protected override Expression VisitMember(MemberExpression node)
        {
            if (node.Expression != null && node.Expression.NodeType == ExpressionType.Parameter)
            {
                var type = node.Expression.Type;
                string columnName;
                PropertyInfo property = type.GetProperty(node.Member.Name);
                if (property.PropertyType.Namespace.Equals("System") && !property.PropertyType.Namespace.Equals("System.Collections.Generic"))
                {
                    ViewMap.ColumnNames.TryGetValue(node.Member.Name, out columnName);
                    _queryBuilder.Append(columnName);
                }
                return node;
            }
            if (node.Expression != null && node.Expression.NodeType == ExpressionType.MemberAccess)
            {
                this.Visit(node.Expression);
            }
            if (node.Expression != null && node.Expression.NodeType == ExpressionType.Constant)
            {
                ConstantExpression constantExpression = node.Expression as ConstantExpression;
                object obj = constantExpression.Value;
                var c = obj.GetType().GetFields().Length;
                if (c > 1)
                {
                    var fields = obj.GetType().GetFields();
                    FieldInfo field = null;
                    foreach (var f in fields)
                    {
                        if (f.Name.Equals(node.Member.Name))
                        {
                            field = f;
                            break;
                        }
                    }
                    if (field != null)
                    {
                        object value = field.GetValue(obj);
                        if (value != null)
                        {
                            if (value.GetType() == typeof(DateTime))
                            {
                                DateTime date = (DateTime)value;
                                value = DataFormater.ParseToSQL(date.ToString("yyyy-MM-dd"));
                                _queryBuilder.Append(value);
                            }
                            else
                            {
                                value = DataFormater.ParseToSQL(value);
                                _queryBuilder.Append(value);
                            }
                        }
                        else
                        {
                            value = DataFormater.ParseToSQL(value);
                            _queryBuilder.Append(value);
                        }
                    }
                }
                else
                {
                    FieldInfo field = obj.GetType().GetFields()[0];
                    object value = field.GetValue(obj);
                    if (value != null)
                    {
                        if (value.GetType() == typeof(DateTime))
                        {
                            DateTime date = (DateTime)value;
                            _queryBuilder.Append(DataFormater.ParseToSQL(date.ToString("yyyy-MM-dd")));
                            return node;
                        }
                    }
                    _queryBuilder.Append(DataFormater.ParseToSQL(value));
                }
            }
            return node;
        }

        protected override Expression VisitConstant(ConstantExpression node)
        {
            if (node.Value == null)
            {
                _queryBuilder.Append("NULL");
                _queryBuilder.Replace("= NULL", "IS NULL");
            }
            else
            {
                _queryBuilder.Append(DataFormater.ParseToSQL(node.Value));
            }
            return node;
        }

        protected override Expression VisitNew(NewExpression node)
        {
            if (typeof(TimeSpan) == node.Type)
            {
                var hour = node.Arguments[0];
                var min = node.Arguments[1];
                var sec = node.Arguments[2];
                TimeSpan time = new TimeSpan(int.Parse(hour + ""), int.Parse(min + ""), int.Parse(sec + ""));
                _queryBuilder.Append(DataFormater.ParseToSQL(time));
            }
            if (typeof(DateTime) == node.Type)
            {
                var age = node.Arguments[0];
                var month = node.Arguments[1];
                var day = node.Arguments[2];
                DateTime date = new DateTime(int.Parse(age + ""), int.Parse(month + ""), int.Parse(day + ""));
                _queryBuilder.Append(DataFormater.ParseToSQL(date.ToString("yyyy-MM-dd")));
            }
            return node;
        }
    }
}
