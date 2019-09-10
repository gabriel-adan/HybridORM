using System.Text;
using System.Reflection;
using System.Linq.Expressions;
using System.Linq;

namespace System.Data.ORM.CoreMap
{
    internal class QueryTranslator<T> : ExpressionVisitor
    {
        IEntityMap Entity;
        StringBuilder _queryBuilder;
        bool ignoreAssignment = false;

        public QueryTranslator(IEntityMap entity)
        {
            Entity = entity;
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
            if (!ignoreAssignment)
            {
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
                if (!property.PropertyType.Namespace.Equals("System") && !property.PropertyType.Namespace.Equals("System.Collections.Generic"))
                {
                    Entity.ForeignKeys.TryGetValue(node.Member.Name, out columnName);
                    if (!string.IsNullOrEmpty(columnName))
                    {
                        _queryBuilder.Append("_this." + columnName);
                    }
                    else
                    {
                        IEntityMap entityMap;
                        Entity.Entities.TryGetValue(node.Member.Name, out entityMap);
                        
                        if (string.IsNullOrEmpty(entityMap.PrimaryKeyName))
                        {
                            string conditions = string.Empty;
                            foreach (var keyValue in Entity.ForeignKeys)
                            {
                                string query = _queryBuilder.ToString();
                                if (keyValue.Key.StartsWith(node.Member.Name) && !query.Contains("_this." + keyValue.Value))
                                {
                                    conditions += "_this." + keyValue.Value + " = {" + keyValue.Key + "} AND ";
                                }
                            }
                            if (!string.IsNullOrEmpty(conditions))
                            {
                                conditions += "-";
                                conditions = conditions.Replace(" AND -", "");
                                _queryBuilder.Append(conditions);
                            }
                            ignoreAssignment = !string.IsNullOrEmpty(conditions);
                        }
                        else
                        {

                        }
                    }
                }
                else
                {
                    Entity.ColumnNames.TryGetValue(node.Member.Name, out columnName);
                    _queryBuilder.Append("_this." + columnName);
                }
                return node;
            }
            if (node.Expression != null && node.Expression.NodeType == ExpressionType.MemberAccess)
            {
                this.Visit(node.Expression);
                MemberExpression expression = node.Expression as MemberExpression;
                string fk;
                Entity.ForeignKeys.TryGetValue(expression.Member.Name + "." + node.Member.Name, out fk);
                if (!string.IsNullOrEmpty(fk))
                {
                    _queryBuilder.Append("_this." + fk);
                }
                else
                {
                    //var property = Entity.Type.GetProperty(expression.Member.Name);
                    //if (property != null)
                    //{
                    //    if (!property.PropertyType.Namespace.Equals("System") && !property.PropertyType.Namespace.Equals("System.Collections.Generic"))
                    //    {
                    //        IEntityMap entityMap = Cfg.Configuration.Mappings.Where(e => e.Value.Type == property.PropertyType).FirstOrDefault().Value;
                    //        if (string.IsNullOrEmpty(entityMap.PrimaryKeyName))
                    //        {

                    //        }
                    //        else
                    //        {

                    //        }
                    //    }
                    //}
                }
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
                    foreach(var f in fields)
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
                                Type type = value.GetType();
                                if (!type.Namespace.Equals("System") && !type.Namespace.Equals("System.Collections.Generic"))
                                {
                                    IEntityMap entityMap = Cfg.Configuration.Mappings.Where(e => e.Value.Type == type).FirstOrDefault().Value;
                                    if (!string.IsNullOrEmpty(entityMap.PrimaryKeyName))
                                    {
                                        object id = entityMap.Type.GetProperty(entityMap.PrimaryKeyName).GetValue(value);
                                        value = DataFormater.ParseToSQL(id);
                                        _queryBuilder.Append(value);
                                    }
                                    else
                                    {
                                        foreach (var keyValue in Entity.ForeignKeys)
                                        {
                                            string query = _queryBuilder.ToString();
                                            string str = keyValue.Key;
                                            if (query.Contains("{" + str + "}"))
                                            {
                                                if (str.Contains("."))
                                                {
                                                    var props = str.Split(".");
                                                    string entityName = props[0];
                                                    string propertyName = props[1];
                                                    var property = type.GetProperty(propertyName);
                                                    if (!property.PropertyType.Namespace.Equals("System") && !property.PropertyType.Namespace.Equals("System.Collections.Generic"))
                                                    {
                                                        var _entity = property.GetValue(value);
                                                        entityMap = Cfg.Configuration.Mappings.Where(e => e.Value.Type == property.PropertyType).FirstOrDefault().Value;
                                                        if (string.IsNullOrEmpty(entityMap.PrimaryKeyName))
                                                        {

                                                        }
                                                        else
                                                        {
                                                            object id = property.PropertyType.GetProperty(entityMap.PrimaryKeyName).GetValue(_entity);
                                                            _queryBuilder.Replace("{" + entityName + "." + propertyName + "}", DataFormater.ParseToSQL(id));
                                                        }
                                                    }
                                                    else
                                                    {
                                                        property = type.GetProperty(propertyName);
                                                        object propertyValue = property.GetValue(value);
                                                        _queryBuilder.Replace("{" + entityName + "." + propertyName + "}", DataFormater.ParseToSQL(propertyValue));
                                                    }
                                                }
                                            }
                                        }
                                        ignoreAssignment = false;
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
