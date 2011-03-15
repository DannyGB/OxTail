/*****************************************************************
*
* Copyright 2011 Dan Beavon
*
* This file is part of OXTail.
*
* OXTail is free software: you can redistribute it and/or modify
* it under the terms of the GNU General Public License as published by
* the Free Software Foundation, either version 3 of the License, or
* (at your option) any later version.
*
* OXTail is distributed in the hope that it will be useful,
* but WITHOUT ANY WARRANTY; without even the implied warranty of
* MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
* GNU General Public License for more details.
*
* You should have received a copy of the GNU General Public License
* along with OxTail.  If not, see <http://www.gnu.org/licenses/>.
* ********************************************************************/

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
