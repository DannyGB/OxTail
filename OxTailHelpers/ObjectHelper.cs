using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;

namespace OxTailHelpers
{
    public static class ObjectHelper
    {
        /// <summary>
        /// Uses reflection to provide a nicely formatted string representation of the property tree of any derived object
        /// </summary>
        /// <param name="obj">The object to reflect</param>
        /// <param name="propertiesToSuppress">list of strings representing properties to suppress reflection of</param>
        /// <returns>A string representation of the property tree of the specified object</returns>
        public static string ReflectToString(this object obj, params string[] propertiesToSuppress)
        {
            BindingFlags flags = System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.FlattenHierarchy;
            System.Reflection.PropertyInfo[] infos = obj.GetType().GetProperties(flags);

            StringBuilder sb = new StringBuilder();

            string typeName = obj.GetType().Name;
            sb.AppendLine();
            sb.Append("{");
            sb.Append(typeName);

            foreach (PropertyInfo info in infos)
            {
                object value = null;
                if (!info.PropertyType.IsPrimitive && Array.Exists<string>(propertiesToSuppress, delegate(string prop) { return info.Name == prop; }))
                {
                    value = "<suppressed> " + info.ReflectedType;
                }
                else
                {
                    value = info.GetValue(obj, null);
                }
                sb.AppendFormat(" [{0} ~ {1}]{2}", info.Name, value != null ? value : "null", System.Environment.NewLine);
            }

            sb.Append("}");
            return sb.ToString();
        }
    }
}
