using System.Text;
using System.Reflection;

namespace System.Data.ORM.CoreMap
{
    static class DataFormater
    {
        public static string ParseToSQL(object value)
        {
            if (value != null)
            {
                Type type = value.GetType();
                if (type == typeof(string) || type == typeof(char) || type == typeof(TimeSpan))
                    return "'" + value + "'";
                if (type == typeof(TimeSpan))
                    return "'" + ((TimeSpan)value).ToString("HH:mm:ss") + "'";
                if (type == typeof(DateTime))
                    return "'" + ((DateTime)value).ToString("yyyy-MM-dd HH:mm:ss") + "'";
                return value.ToString();
            }
            return "NULL";
        }

        public static object ParseToData(PropertyInfo property, object data)
        {
            if (data != null)
            {
                if (data.GetType() == typeof(DBNull))
                    return data as Nullable;
                if (data.GetType() == typeof(SByte))
                {
                    return data.ToString().Equals("1");
                }
                if (property.PropertyType == typeof(TimeSpan) && data.GetType() == typeof(DateTime))
                {
                    DateTime date = (DateTime)data;
                    return date.TimeOfDay;
                }
                if (data.GetType() == typeof(byte[]) && property.PropertyType == typeof(string))
                {
                    return Encoding.UTF8.GetString((byte[])data, 0, ((byte[])data).Length);
                }
            }
            return data;
        }
    }
}
